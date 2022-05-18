using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public sealed class CommandStepTriggerBinding
        : ITriggerBinding
    {

        private readonly ParameterInfo parameter = null;
        private readonly CommandStepTriggerAttribute triggerAttribute = null;

        public CommandStepTriggerBinding(ParameterInfo parameter, CommandStepTriggerAttribute triggerAttribute)
        {
            this.parameter = parameter;
            this.triggerAttribute = triggerAttribute;
        }

        public Type TriggerValueType => throw new NotImplementedException();

        public IReadOnlyDictionary<string, Type> BindingDataContract => throw new NotImplementedException();

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
            throw new NotImplementedException();
        }
    }
}
