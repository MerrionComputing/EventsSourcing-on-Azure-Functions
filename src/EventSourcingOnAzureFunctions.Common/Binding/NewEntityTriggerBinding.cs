using EventSourcingOnAzureFunctions.Common.Listener;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// A binding to trigger an azure function when a new entity is created (by having the first event added to 
    /// the event stream for that entity)
    /// </summary>
    public class NewEntityTriggerBinding
        : ITriggerBinding
    {
        private readonly ParameterInfo parameter = null;
        private readonly NewEntityTriggerAttribute triggerAttribute = null;


        public NewEntityTriggerBinding(ParameterInfo parameter, 
            NewEntityTriggerAttribute triggerAttribute)
        {
            this.parameter = parameter;
            this.triggerAttribute = triggerAttribute;
        }

        public Type TriggerValueType => typeof(NewEntityContext);

        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>
        {
            {"headers", typeof(Dictionary<string, string>) }
        };

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            throw new NotImplementedException();
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return Task.FromResult<IListener>(new NewEntityTriggerListener(context.Executor, triggerAttribute));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Prompt = "NewEntityCreated",
                    Description = "New Entity trigger fired",
                    DefaultValue = "Sample"
                }
            };
        }
    }

}
