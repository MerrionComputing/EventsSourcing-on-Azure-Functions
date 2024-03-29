﻿using EventSourcingOnAzureFunctions.Common.Listener;
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

    public sealed class CommandStepTriggerAttributeBinding
         : ITriggerBinding
    {
        private readonly ParameterInfo parameter = null;
        private readonly CommandStepTriggerAttribute commandAttribute = null;

        public Type TriggerValueType => throw new NotImplementedException();

        public IReadOnlyDictionary<string, Type> BindingDataContract => throw new NotImplementedException();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            throw new NotImplementedException();
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
           if (context == null)
           {
               throw new ArgumentNullException(nameof(context));
           }

            return Task.FromResult<IListener>(new CommandListener(context.Executor, commandAttribute));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Prompt = "CommandStep",
                    Description = "Command Step trigger fired",
                    DefaultValue = "Sample"
                }
            };
        }
    }
}
