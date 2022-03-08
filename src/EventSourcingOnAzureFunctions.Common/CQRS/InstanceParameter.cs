using EventSourcingOnAzureFunctions.Common.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.CQRS
{
    /// <summary>
    /// A parameter linked to an instance of a command or query
    /// </summary>
    public sealed class InstanceParameter
    {

        /// <summary>
        /// Is the parameter target a Command or a Query
        /// </summary>
        /// <remarks>
        /// This may be extended to projection and classifier so kept as a string
        /// </remarks>
        public string TargetType { get; set; }

        /// <summary>
        /// The domain name the command or query is running in
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The name of the query or command being run
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The command or query instance
        /// </summary>
        public string InstanceKey { get; set; }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// The value of the parameter being set
        /// </summary>
        public object ParameterValue { get; set; }

        public InstanceParameter(CommandAttribute cmd, 
            string parameterName, 
            object parameterValue)
        {
            TargetType = "Command";
            DomainName = cmd.DomainName;
            Name = cmd.CommandName;
            InstanceKey = cmd.UniqueIdentifier;
            ParameterName = parameterName;
            ParameterValue = parameterValue;
        }

        public InstanceParameter(QueryAttribute qry,
            string parameterName,
            object parameterValue)
        {
            TargetType = "Query";
            DomainName = qry.DomainName;
            Name = qry.QueryName;
            InstanceKey = qry.UniqueIdentifier;
            ParameterName = parameterName;
            ParameterValue = parameterValue;
        }

        /// <summary>
        /// Parameter-less constructor for serialisation
        /// </summary>
        public InstanceParameter()
        {
        }
    }
}
