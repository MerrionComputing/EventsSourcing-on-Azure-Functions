using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    /// <summary>
    /// A write context to wrap around an event in order to give additional non-business context for the event 
    /// </summary>
    public class WriteContext
        : IWriteContext
    {
        public string Who { get; }

        public string Source { get; internal set; }

        public string Commentary { get; }

        public string CorrelationIdentifier { get; internal set; }

        public string CausationIdentifier { get; internal set; }


        public string SchemaName { get; internal set; }

        public static WriteContext DefaultWriterContext()
        {
            // todo - set default values... maybe from config or..?
            return new WriteContext();
        }

        internal static IWriteContext CreateFunctionContext(FunctionBindingContext functionContext)
        {
            WriteContext ret = DefaultWriterContext();
            if (null != ret)
            {
                if (null != functionContext)
                {
                    ret.CausationIdentifier  = functionContext.FunctionInstanceId.ToString("D");
                    ret.Source = functionContext.MethodName;
                }
            }
            return ret;
        }
    }
}
