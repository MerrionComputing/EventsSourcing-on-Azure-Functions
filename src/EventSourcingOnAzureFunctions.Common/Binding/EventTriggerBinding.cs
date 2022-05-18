using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public class EventTriggerBinding
        : ITriggerBinding
    {

        // TODO: https://github.com/Azure/azure-functions-durable-extension/blob/dev/src/WebJobs.Extensions.DurableTask/Bindings/ActivityTriggerAttributeBindingProvider.cs
        
        private readonly  EventTriggerAttributeBindingProvider eventTriggerAttributeBindingProvider;
        private readonly ParameterInfo parameter;
        private readonly EventTriggerAttribute trigger;
        private readonly IReadOnlyDictionary<string, Type> contract;

        public EventTriggerBinding(EventTriggerAttributeBindingProvider eventTriggerAttributeBindingProvider, 
            ParameterInfo parameter, 
            EventTriggerAttribute trigger)
        {
            this.eventTriggerAttributeBindingProvider = eventTriggerAttributeBindingProvider;
            this.parameter = parameter;
            this.trigger = trigger;

            // Create the data binding contract..
            contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
                {
                    // This binding supports return values of any type
                    { "$return", typeof(object).MakeByRefType() },
                    { "instanceid", typeof(string) },
                };


        }

        public Type TriggerValueType => throw new NotImplementedException();


        public IReadOnlyDictionary<string, Type> BindingDataContract => contract ;

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            throw new NotImplementedException();
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            throw new NotImplementedException();
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Prompt = "NewEventAppended",
                    Description = "New Event trigger fired",
                    DefaultValue = "Sample"
                }
            };
        }
    }
}
