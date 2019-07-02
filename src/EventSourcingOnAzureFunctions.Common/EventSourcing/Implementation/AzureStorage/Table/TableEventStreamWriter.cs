using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.CosmosDB.Table;
using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        private IWriteContext _writerContext;
        public void SetContext(IWriteContext writerContext)
        {
            _writerContext = writerContext;
        }


        public TableEventStreamWriter(IEventStreamIdentity identity,
            string connectionStringName = @"")
            : base(identity, true, connectionStringName)
        {

        }



    }
}
