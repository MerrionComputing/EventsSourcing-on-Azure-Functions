using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public sealed class TableEventStreamWriter
        : TableEventStreamBase, IEventStreamWriter
    {


        public async Task AppendEvent(IEvent eventInstance,
            int expectedTopSequenceNumber = 0,
            int eventVersionNumber = 1,
            EventStreamExistenceConstraint streamConstraint = EventStreamExistenceConstraint.Loose)
        {

            int nextSequence = 0;

            // Read and update the [RECORDID_SEQUENCE] row in a transaction..
            nextSequence =  IncrementSequenceNumber();

            if (expectedTopSequenceNumber > 0)
            {
                if ((1 + expectedTopSequenceNumber) < nextSequence)
                {
                    //Concurrency error has occured
                    throw new EventStreamWriteException(this,
                    (nextSequence - 1),
                    message: $"Out of sequence write - expected seqeunce number {expectedTopSequenceNumber}, actual {nextSequence - 1}",
                    source: "Table Event Stream Writer");
                }
            }


            var dteEvent = MakeDynamicTableEntity(eventInstance, nextSequence);
            if (null != dteEvent )
            {
                await base.Table.ExecuteAsync(TableOperation.Insert(dteEvent));
            }

        }



        /// <summary>
        /// Increment the sequence number for this event stream and return the new number
        /// </summary>
        /// <remarks>
        /// This is done before the event itself is written so that a partial failure leaves a gap in the event stream which is
        /// less harmful than an overwritten event record
        /// </remarks>
        private int IncrementSequenceNumber()
        {
            bool recordUpdated = false;
            int tries = 0;

            TableEntityKeyRecord streamFooter = null;

            while (!recordUpdated)
            {
                tries += 1;
                // read in the a [TableEntityKeyRecord]
                
                streamFooter = (TableEntityKeyRecord)Table.Execute(TableOperation.Retrieve<TableEntityKeyRecord>(this.InstanceKey, SequenceNumberAsString(0))).Result;
                if (null == streamFooter)
                {
                    streamFooter = new TableEntityKeyRecord(this);
                }
                streamFooter.LastSequence += 1;

                string lastETag = streamFooter.ETag;


                TableResult tres = Table.Execute(TableOperation.InsertOrReplace(streamFooter),
                      null,
                      new OperationContext
                      {
                          UserHeaders = new Dictionary<String, String>
                          {
                              { "If-Match", lastETag }
                          }
                      });

                if (tres.HttpStatusCode == 204)
                {
                    recordUpdated = true;
                }

                if (tries > 200)
                {
                    // catastrophic deadlock
                    throw new EventStreamWriteException(this, streamFooter.LastSequence,
                        message: "Unable to increment the stream sequence number due to deadlock",
                        source: "Table Event Stream Writer"); 
                }
            }

            if (null != streamFooter)
            {
                return streamFooter.LastSequence;
            }
            else
            {
                return 1;
            }
        }

        private IWriteContext _writerContext;
        public void SetContext(IWriteContext writerContext)
        {
            _writerContext = writerContext;
        }

        public DynamicTableEntity MakeDynamicTableEntity(IEvent eventToPersist,
    int sequenceNumber)
        {

            DynamicTableEntity ret = new DynamicTableEntity(this.InstanceKey,
                SequenceNumberAsString(sequenceNumber));

            // Add the event type
            ret.Properties.Add(FIELDNAME_EVENTTYPE,
                  new EntityProperty(EventNameAttribute.GetEventName(eventToPersist.GetType())));

            if (null != _writerContext )
            {
                if (! string.IsNullOrWhiteSpace(_writerContext.Commentary ) )
                {
                    ret.Properties.Add(FIELDNAME_COMMENTS,
                        new EntityProperty(_writerContext.Commentary));
                }
                if (! string.IsNullOrWhiteSpace(_writerContext.CorrelationIdentifier ) )
                {
                    ret.Properties.Add(FIELDNAME_CORRELATION_IDENTIFIER,
                        new EntityProperty(_writerContext.CorrelationIdentifier ));
                }
                if (! string.IsNullOrWhiteSpace(_writerContext.Source ) )
                {
                    ret.Properties.Add(FIELDNAME_SOURCE,
                        new EntityProperty(_writerContext.Source ));
                }
                if (! string.IsNullOrWhiteSpace(_writerContext.Who ) )
                {
                    ret.Properties.Add(FIELDNAME_WHO,
                        new EntityProperty(_writerContext.Who ));
                } 
            }

            if (null != eventToPersist.EventPayload )
            {
                // save the payload properties 
                int propertiesCount = 0;
                foreach (System.Reflection.PropertyInfo pi in eventToPersist.EventPayload.GetType().GetProperties())
                {
                    if (pi.CanRead )
                    {
                        if (! IsContextProperty(pi.Name) )
                        {
                            if (propertiesCount > MAX_FREE_DATA_FIELDS)
                            {
                                throw new EventStreamWriteException(this,
                                    sequenceNumber,
                                    $"Event has too many fields to store in an Azure table"); 
                            }
                            else
                            {
                                if (!IsPropertyEmpty(pi, eventToPersist.EventPayload))
                                {
                                    ret.Properties.Add(pi.Name, MakeEntityProperty(pi, eventToPersist.EventPayload));
                                }
                            }
                        }
                    }
                }
            }

            return ret;

        }

        public static  EntityProperty MakeEntityProperty(PropertyInfo pi, 
            object eventPayload)
        {
            
            if (null == eventPayload )
            {
                return new EntityProperty(string.Empty);
            }

            //cast it to the correct type?
            if (pi.PropertyType == typeof(bool))
            {
                return new EntityProperty((bool)pi.GetValue(eventPayload, null));
            }

            if (pi.PropertyType == typeof(double))
            {
                return new EntityProperty((double)pi.GetValue(eventPayload, null));
            }

            if (pi.PropertyType == typeof(int))
            {
                return new EntityProperty((int)pi.GetValue(eventPayload, null));
            }

            if (pi.PropertyType == typeof(long))
            {
                return new EntityProperty((long)pi.GetValue(eventPayload, null));
            }

            if (pi.PropertyType == typeof(DateTimeOffset ))
            {
                return new EntityProperty((DateTimeOffset )pi.GetValue(eventPayload, null));
            }

            if (pi.PropertyType == typeof(Guid))
            {
                return new EntityProperty((Guid)pi.GetValue(eventPayload, null));
            }

            if (pi.PropertyType == typeof(DateTime))
            {
                return new EntityProperty((DateTime )pi.GetValue(eventPayload, null));
            }

            if (pi.PropertyType == typeof(byte[]))
            {
                return new EntityProperty((byte[])pi.GetValue(eventPayload, null));
            }

            if (pi.PropertyType == typeof(decimal))
            {
                decimal oVal = (decimal)pi.GetValue(eventPayload, null);
                return new EntityProperty((double)oVal);
            }

            return new EntityProperty(pi.GetValue(eventPayload, null).ToString());
        }

        public TableEventStreamWriter(IEventStreamIdentity identity,
            string connectionStringName = @"")
            : base(identity, true, connectionStringName)
        {

        }



    }
}
