using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{

    /// <summary>
    /// Output binding provider to select the event stream on which to append 
    /// events or run projections or classifiers.
    /// </summary>
    public sealed class EventStreamAttributeBindingProvider
        : IBindingProvider
    {

        private readonly IEventStreamSettings _eventStreamSettings;

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Determine whether we should bind to the current parameter
            ParameterInfo parameter = context.Parameter;
            EventStreamAttribute attribute = parameter.GetCustomAttribute<EventStreamAttribute>(inherit: false);

            if (attribute == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            // This can only bind to EventStream
            if (!(parameter.ParameterType == typeof(EventStream)))
            {
                throw new InvalidOperationException(
                        $"Can't bind EventStreamAttribute to type '{parameter.ParameterType}'.");
            }

            return Task.FromResult<IBinding>(new EventStreamAttributeBinding(parameter, _eventStreamSettings ));

        }

        public EventStreamAttributeBindingProvider(IEventStreamSettings eventStreamSettings)
        {
            _eventStreamSettings = eventStreamSettings;
        }
    }

}
