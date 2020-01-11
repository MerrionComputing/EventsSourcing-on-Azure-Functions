using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.CQRS.Common.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.CQRS
{
    /// <summary>
    /// A wrapper class used to trigger and manage a query orchestration
    /// </summary>
    public class Query
    {

        private readonly IWriteContext _queryContext;

        private readonly string _domainName;
        /// <summary>
        /// The business domain in which the query is located
        /// </summary>
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        private readonly string _queryName;
        /// <summary>
        /// The name of the query to run
        /// </summary>
        public string QueryName
        {
            get
            {
                return _queryName;
            }
        }

        private readonly string _uniqueIdentifier;
        /// <summary>
        /// The unique instance of the command to run
        /// </summary>
        public string UniqueIdentifier
        {
            get
            {
                return _uniqueIdentifier;
            }
        }

        // Parameters
        public async Task SetParameter(string parameterName, object parameterValue)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {

                EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                    QueryName,
                    UniqueIdentifier),
                    context: _queryContext);

                ParameterValueSet evParam = new ParameterValueSet()
                {
                    Name = parameterName,
                    Value = parameterValue
                };

                await esQry.AppendEvent(evParam);
            }
        }

        // Validations

        // Classification request

        // Projection request

        // Collations

        // Response
        private async Task SetResponseTarget(string targetType,
            string targetLocation)
        {
            if ((!string.IsNullOrWhiteSpace(targetType )) && (!string.IsNullOrWhiteSpace(targetLocation )))
            {

                EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                    QueryName,
                    UniqueIdentifier),
                    context: _queryContext);

                OutputLocationSet evParam = new OutputLocationSet()
                {
                    TargetType  = targetType ,
                    Location  = targetLocation 
                };

                await esQry.AppendEvent(evParam);
            }
        }

        /// <summary>
        /// Add an azure storage blob target for the query to save its results
        /// into
        /// </summary>
        public async Task AddStorageBlobOutput(Uri containerAddress,
            Uri targetFilename)
        {
            await SetResponseTarget("Azure Storage Blob",
                new Uri(containerAddress, targetFilename).AbsoluteUri); 
        }

        /// <summary>
        /// Add an email output target for the query results
        /// </summary>
        /// <param name="emailAddress">
        /// The address to which to send the query results
        /// </param>
        public async Task AddEmailOutput(string emailAddress)
        {
            await SetResponseTarget("Email Address",
                emailAddress );
        }

        /// <summary>
        /// Add a webhook output target for the query results
        /// </summary>
        /// <param name="webhookAddress">
        /// The URI of the web hook to which to post results
        /// </param>
        public async Task AddWebhookOutput(Uri webhookAddress)
        {
            await SetResponseTarget("Webhook",
                webhookAddress.AbsoluteUri );
        }
        


        public Query(QueryAttribute attribute)
        {
            if (null != attribute )
            {
                _domainName = attribute.DomainName;
                _queryName = attribute.QueryName;
                _uniqueIdentifier = attribute.UniqueIdentifier;
                // Make a query context
                _queryContext = new WriteContext()
                {
                    Source = _queryName ,
                    CausationIdentifier = _uniqueIdentifier
                };
            }
        }


        /// <summary>
        /// Make a "queries" domain for the given top level domain
        /// </summary>
        /// <param name="domainName">
        /// The top level (business) domain
        /// </param>
        /// <remarks>
        /// This allows different domains' query names to be unique even if
        /// the base query names are not
        /// </remarks>
        public static string MakeDomainQueryName(string domainName)
        {
            if (!string.IsNullOrWhiteSpace(domainName))
            {
                return domainName.Trim() + @".Query";
            }
            else
            {
                return "Query";
            }
        }
    }
}
