using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.File
{
    public sealed class FileEventStreamWriter
        : FileEventStreamBase , IEventStreamWriter
    {


        public async Task<IAppendResult> AppendEvent(IEvent eventInstance,
            int expectedTopSequenceNumber = 0,
            int eventVersionNumber = 1,
            EventStreamExistenceConstraint streamConstraint = EventStreamExistenceConstraint.Loose)
        {

            // Get the next sequence number
            int nextSequence = 0;

            // check stream constraints
            if (streamConstraint != EventStreamExistenceConstraint.Loose)
            {
                // find out if the stream exists
                bool exists = await StreamAlreadyExists();
                if (streamConstraint == EventStreamExistenceConstraint.MustExist)
                {
                    if (!exists)
                    {
                        throw new EventStreamWriteException(this,
                     0,
                    message: $"Stream is constrained to MustExist but has not been created",
                    source: "File Event Stream Writer");
                    }
                }
                if (streamConstraint == EventStreamExistenceConstraint.MustBeNew)
                {
                    if (exists)
                    {
                        throw new EventStreamWriteException(this,
                     0,
                    message: $"Stream is constrained to be new but has already been created",
                    source: "File Event Stream Writer");
                    }
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete all the files for a given event stream
        /// </summary>
        public Task DeleteStream()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Exists()
        {
            throw new NotImplementedException();
        }

        public void SetContext(IWriteContext writerContext)
        {
            throw new NotImplementedException();
        }

        public Task WriteIndex()
        {
            throw new NotImplementedException();
        }




        /// <summary>
        /// Constructor to create a n Azure files backed writer for a given event stream
        /// </summary>
        /// <param name="identity">
        /// The identity of the entity for which to create an event stream writer
        /// </param>
        /// <param name="connectionStringName">
        /// (Optional) The name of the connection string to use to access the azure storage
        /// </param>
        public FileEventStreamWriter(IEventStreamIdentity identity,
            string connectionStringName = @"")
            : base(identity, true, connectionStringName)
        {

        }
    }
}
