using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.File;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.File
{
    public sealed class FileEventStreamReader
           : FileEventStreamBase,
           IEventStreamReader
    {

        private readonly IEventMaps _eventMaps = null;


        public Task<bool> Exists()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEvent>> GetAllEvents()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetAllInstanceKeys(DateTime? asOfDate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEvent>> GetEvents(int StartingSequenceNumber = 0,
            DateTime? effectiveDateTime = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEventContext>> GetEventsWithContext(int StartingSequenceNumber = 0,
            DateTime? effectiveDateTime = null)
        {
            throw new NotImplementedException();
        }


        public FileEventStreamReader(IEventStreamIdentity identity,
    string connectionStringName = @"",
    IEventMaps eventMaps = null)
    : base(identity, false, connectionStringName)
        {


            // if event maps not passed in you have to create a default
            if (null == eventMaps)
            {
                _eventMaps = EventMaps.CreateDefaultEventMaps();
            }
            else
            {
                _eventMaps = eventMaps;
            }
        }


        /// <summary>
        /// Creates an azure file storage based event stream reader for the given aggregate
        /// </summary>
        /// <param name="identity">
        /// The unique identifier of the event stream to read
        /// </param>
        /// <param name="connectionStringName">
        /// The name of the connection string to use to do the reading from the files
        /// </param>
        public static FileEventStreamReader Create(IEventStreamIdentity identity,
            string connectionStringName = @"")
        {
            return new FileEventStreamReader(identity, connectionStringName);
        }

        public static ProjectionProcessor CreateProjectionProcessor(IEventStreamIdentity identity,
            string connectionStringName = @"")
        {
            return new ProjectionProcessor(Create(identity, connectionStringName));
        }

        public static ClassificationProcessor CreateClassificationProcessor(IEventStreamIdentity identity,
              string connectionStringName = @"")
        {
            return new ClassificationProcessor(Create(identity, connectionStringName));
        }
    }
}
