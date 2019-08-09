using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// Binding provider to bind the [EventTrigger] attribute
    /// </summary>
    public class EventTriggerAttributeBindingProvider
        : ITriggerBindingProvider
    {
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;
            EventTriggerAttribute trigger = parameter.GetCustomAttribute<EventTriggerAttribute>(inherit: false);
            if (trigger == null)
            {
                // We could not bind to an event trigger as there is no attribute so return NULL
                return Task.FromResult<ITriggerBinding>(null);
            }

            // TODO: Get the details of the event(s) to bind to from the attribute

            // TODO: Create the binding
            var binding = new EventTriggerBinding(this, parameter, trigger);
            return Task.FromResult<ITriggerBinding>(binding);

        }
    }
}
