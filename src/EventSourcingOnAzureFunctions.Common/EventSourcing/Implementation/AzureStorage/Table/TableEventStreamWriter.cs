using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public sealed class TableEventStreamWriter
        : TableEventStreamBase, IEventStreamWriter
    {

        /// <summary>
        /// Save an event onto the end of the event stream stored in Azure table
        /// </summary>
        /// <param name="eventInstance">
        /// The specific event to append to the end of the store
        /// </param>
        /// <param name="expectedTopSequenceNumber">
        /// If this is set and the sequence number of the event stream is higher then the event is 
        /// not written
        /// </param>
        /// <param name="eventVersionNumber">
        /// The version number of the event being written
        /// </param>
        /// <param name="streamConstraint">
        /// Additional constraint that must be true if the event is to be appended
        /// </param>
        public async Task<IAppendResult> AppendEvent(IEvent eventInstance,
            int expectedTopSequenceNumber = 0,
            int eventVersionNumber = 1,
            EventStreamExistenceConstraint streamConstraint = EventStreamExistenceConstraint.Loose)
        {

            int nextSequence = 0;

            // check stream constraints
            if (streamConstraint != EventStreamExistenceConstraint.Loose )
            {
                // find out if the stream exists
                bool exists = await this.StreamAlreadyExists();
                if (streamConstraint== EventStreamExistenceConstraint.MustExist )
                {
                    if (! exists )
                    {
                        throw new EventStreamWriteException(this,
                     0,
                    message: $"Stream is constrained to MustExist but has not been created",
                    source: "Table Event Stream Writer");
                    }
                }
                if (streamConstraint == EventStreamExistenceConstraint.MustBeNew )
                {
                    if (exists)
                    {
                        throw new EventStreamWriteException(this,
                     0,
                    message: $"Stream is constrained to be new but has already been created",
                    source: "Table Event Stream Writer");
                    }
                }
            }

            // Read and update the [RECORDID_SEQUENCE] row in a transaction..
            nextSequence = await IncrementSequenceNumber();

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


            TableEntity dteEvent = MakeDynamicTableEntity(eventInstance, nextSequence);
            if (null != dteEvent)
            {
                await base.Table.AddEntityAsync(dteEvent);
            }

            return new AppendResult((nextSequence == 1), nextSequence);

        }



        /// <summary>
        /// Increment the sequence number for this event stream and return the new number
        /// </summary>
        /// <remarks>
        /// This is done before the event itself is written so that a partial failure leaves a gap in the event stream which is
        /// less harmful than an overwritten event record
        /// </remarks>
        private async Task<int> IncrementSequenceNumber()
        {
            bool recordUpdated = false;
            int tries = 0;

            TableEntityKeyRecord streamFooter = null;

            while (!recordUpdated)
            {
                tries += 1;
                // read in the a [TableEntityKeyRecord]

                await Table.CreateIfNotExistsAsync();

                try
                {
                    streamFooter = await Table.GetEntityAsync<TableEntityKeyRecord>(this.InstanceKey, SequenceNumberAsString(0));
                }
                catch (Azure.RequestFailedException ex)
                {
                    // Need to create a new stream footer if not found...but rethrow any other error
                    if (ex.Status != 404)
                    {
                        throw;
                    }
                }

                if (null == streamFooter)
                {
                    streamFooter = new TableEntityKeyRecord(this);
                    // create an index card...
                    await WriteIndex();
                }
                streamFooter.LastSequence += 1;

                try
                {
                    Azure.Response tres;
                    if (streamFooter.ETag.ToString() == "" )
                    {
                        tres =await Table.AddEntityAsync(streamFooter);
                    }
                    else
                    {
                        tres = await Table.UpdateEntityAsync(streamFooter, streamFooter.ETag );
                    }
                    
                    if (tres.Status  == 204)
                    {
                        recordUpdated = true;
                    }
                }
                catch (Azure.RequestFailedException  rEx)
                {
                    if (rEx.Status == (int)HttpStatusCode.PreconditionFailed)
                    {
                        // Precondition Failed - could not update the footer due to a concurrency 🐊 error
                        recordUpdated = false;
                        // Wait a random-ish amount of time
                        int delayMilliseconds = 13 * new Random().Next(10, 100);
                        await Task.Delay(delayMilliseconds); 
                    }
                    else
                    {
                        throw new EventStreamWriteException(this, streamFooter.LastSequence,
                                                message: "Unable to increment the stream sequence number due to storage error",
                                                source: "Table Event Stream Writer",
                                                innerException: rEx);
                    }
                }

                if (tries > 500)
                {
                    // catastrophic deadlock
                    throw new EventStreamWriteException(this, streamFooter.LastSequence,
                        message: "Unable to increment the stream sequence number due to deadlock",
                        source: "Table Event Stream Writer");
                }
            }

            if (null != streamFooter)
            {
                if (streamFooter.Deleting )
                {
                    // Do not allow a write to an event stream that is being deleted
                    throw new EventStreamWriteException(this, 
                        streamFooter.LastSequence,
                        message: "Unable to write to this event stream as it is being deleted",
                        source: "Table Event Stream Writer");
                }
                return streamFooter.LastSequence;
            }
            else
            {
                return 1;
            }
        }


        /// <summary>
        /// Delete all the records in the table linked to this event stream
        /// </summary>
        public async Task  DeleteStream()
        {
            // 1- mark the stream footer as "Deleting"
            bool recordUpdated = false;
            int tries = 0;

            TableEntityKeyRecord streamFooter = null;

            while (!recordUpdated)
            {
                tries += 1;
                // read in the a [TableEntityKeyRecord]
                streamFooter = await Table.GetEntityAsync<TableEntityKeyRecord>(this.InstanceKey, SequenceNumberAsString(0));

                if (null == streamFooter)
                {
                    streamFooter = new TableEntityKeyRecord(this);
                }
                streamFooter.Deleting = true;


                try
                {
                    Azure.Response tres =  await Table.UpdateEntityAsync(streamFooter, streamFooter.ETag);

                    if (tres.Status  == 204)
                    {
                        recordUpdated = true;
                    }
                }
                catch (Azure.RequestFailedException rEx)
                {
                    if (rEx.Status == (int)HttpStatusCode.PreconditionFailed)
                    {
                        // Precondition Failed - could not update the footer due to a concurrency error
                        recordUpdated = false;
                        // Wait a random-ish amount of time
                        int delayMilliseconds = 13 * new Random().Next(10, 100);
                        System.Threading.Thread.Sleep(delayMilliseconds);
                    }
                    else
                    {
                        throw new EventStreamWriteException(this, streamFooter.LastSequence,
                                                message: "Unable to set the Deleting flag stream sequence number due to storage error",
                                                source: "Table Event Stream Writer",
                                                innerException: rEx);
                    }
                }

                if (tries > 500)
                {
                    // catastrophic deadlock
                    throw new EventStreamWriteException(this, streamFooter.LastSequence,
                        message: "Unable to set the Deleting flag number due to deadlock",
                        source: "Table Event Stream Writer");
                }
            }

            // 2- delete the actual stream records in reverse order
            if (Table != null)
            {
                Azure.Pageable<TableEntity> getEventsToDeleteQuery = Table.Query<TableEntity>(filter: DeleteRowsQuery());

                int currentRow = 0;

                var batchedActions = new List<TableTransactionAction>();
                foreach (var entity in getEventsToDeleteQuery)
                {
                    batchedActions.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity));

                    if (currentRow % MAX_BATCH_SIZE == 0)
                    {
                        // may need to split batches into groups of 100 actions
                    }
                }

                // Submit the batch.
                await Table.SubmitTransactionAsync(batchedActions).ConfigureAwait(false);

            }

            // 3 - delete the index card
            if (Table != null)
            {
                try
                {
                     Azure.Response resp =  await Table.DeleteEntityAsync(TableEntityIndexCardRecord.INDEX_CARD_PARTITION, this.InstanceKey);
                }
                catch (Azure.RequestFailedException ex)
                {
                    // Need to create a new stream footer if not found...but rethrow any other error
                    if (ex.Status != 404)
                    {
                        throw;
                    }
                }

            }
        }

        /// <summary>
        /// Create a query to get the events 
        /// for an instance key to delete them
        /// </summary>
        private string  DeleteRowsQuery()
        {
            return TableClient.CreateQueryFilter($"PartitionKey eq {InstanceKey}");
        }

        private IWriteContext _writerContext;
        public void SetContext(IWriteContext writerContext)
        {
            _writerContext = writerContext;
        }

        public TableEntity MakeDynamicTableEntity(IEvent eventToPersist,
    int sequenceNumber)
        {

            TableEntity ret = new TableEntity(this.InstanceKey,
                SequenceNumberAsString(sequenceNumber));

            // Add the event type
            if (!string.IsNullOrWhiteSpace(eventToPersist.EventTypeName))
            {
                // Use the event type name given 
                ret.Add(FIELDNAME_EVENTTYPE,
                      eventToPersist.EventTypeName); 
            }
            else
            {
                // fall back on the .NET name of the payload class
                ret.Add(FIELDNAME_EVENTTYPE,
                      EventNameAttribute.GetEventName(eventToPersist.GetType()));
            }

            if (null != _writerContext)
            {
                if (!string.IsNullOrWhiteSpace(_writerContext.Commentary))
                {
                    ret.Add(FIELDNAME_COMMENTS,
                        _writerContext.Commentary);
                }
                if (!string.IsNullOrWhiteSpace(_writerContext.CorrelationIdentifier))
                {
                    ret.Add(FIELDNAME_CORRELATION_IDENTIFIER,
                        _writerContext.CorrelationIdentifier);
                }
                if (!string.IsNullOrWhiteSpace(_writerContext.Source))
                {
                    ret.Add(FIELDNAME_SOURCE,
                        _writerContext.Source);
                }
                if (!string.IsNullOrWhiteSpace(_writerContext.Who))
                {
                    ret.Add(FIELDNAME_WHO,
                        _writerContext.Who);
                }
                if (! string.IsNullOrWhiteSpace(_writerContext.SchemaName  ) )
                {
                    ret.Add(FIELDNAME_SCHEMA,
                        _writerContext.SchemaName);  
                }
            }

            if (null != eventToPersist.EventPayload)
            {
                // save the payload properties 
                int propertiesCount = 0;
                foreach (System.Reflection.PropertyInfo pi in eventToPersist.EventPayload.GetType().GetProperties())
                {
                    if (pi.CanRead)
                    {
                        if (!IsContextProperty(pi.Name))
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
                                    ret.Add(pi.Name, MakeEntityProperty(pi, eventToPersist.EventPayload));
                                }
                            }
                        }
                    }
                }
            }

            return ret;

        }




        /// <summary>
        /// Constructor to create a table backed writer for a given event stream
        /// </summary>
        /// <param name="identity">
        /// The identity of the entity for which to create an event stream writer
        /// </param>
        /// <param name="connectionStringName">
        /// (Optional) The name of the connection string to use to access the azure storage
        /// </param>
        public TableEventStreamWriter(IEventStreamIdentity identity,
    string connectionStringName = @"")
    : base(identity, true, connectionStringName)
        {

        }


        public static object MakeEntityProperty(PropertyInfo pi,
            object eventPayload)
        {

            if (null == eventPayload)
            {
                return string.Empty;
            }

            //cast it to the correct type?
            if (pi.PropertyType == typeof(bool))
            {
                return (bool)pi.GetValue(eventPayload, null);
            }

            if (pi.PropertyType == typeof(double))
            {
                return (double)pi.GetValue(eventPayload, null);
            }

            if (pi.PropertyType == typeof(int))
            {
                return (int)pi.GetValue(eventPayload, null);
            }

            if (pi.PropertyType == typeof(long))
            {
                return (long)pi.GetValue(eventPayload, null);
            }

            if (pi.PropertyType == typeof(DateTimeOffset))
            {
                return (DateTimeOffset)pi.GetValue(eventPayload, null);
            }

            if (pi.PropertyType == typeof(Guid))
            {
                return (Guid)pi.GetValue(eventPayload, null);
            }

            if (pi.PropertyType == typeof(DateTime))
            {
                return (DateTime)pi.GetValue(eventPayload, null);
            }

            if (pi.PropertyType == typeof(byte[]))
            {
                return (byte[])pi.GetValue(eventPayload, null);
            }

            if (pi.PropertyType == typeof(decimal))
            {
                decimal oVal = (decimal)pi.GetValue(eventPayload, null);
                return (double)oVal;
            }

            return pi.GetValue(eventPayload, null).ToString();
        }

        /// <summary>
        /// Write (or overwrite) an INDEX-CARD row for this entity
        /// </summary>
        public async Task WriteIndex()
        {
            // Make an index card record
            TableEntityIndexCardRecord indexCard = new TableEntityIndexCardRecord()
            {
            DomainName = this.DomainName ,
            EntityTypeName = this.EntityTypeName ,
            InstanceKey = this.InstanceKey 
            };

            try
            {
                await Table.AddEntityAsync<TableEntityIndexCardRecord>(indexCard);
            } 
            catch (Azure.RequestFailedException ex)
            {
                if (ex.Status != 409)
                {
                    throw;
                }
            }

        }
    }
}
