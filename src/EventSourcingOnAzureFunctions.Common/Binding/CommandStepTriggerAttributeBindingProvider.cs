using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{

        /// <summary>
        /// The provider to bind a NewEntityAttribute to the parameter/function triggered 
        /// when a new entity is created
        /// </summary>
        /// <remarks>
        /// See https://www.tpeczek.com/2018/11/azure-functions-20-extensibility_20.html
        /// </remarks>
        public class CommandStepTriggerAttributeBindingProvider
            : ITriggerBindingProvider
        {

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;

            CommandStepTriggerAttribute triggerAttribute = parameter.GetCustomAttribute<CommandStepTriggerAttribute>(inherit: false);
            if (triggerAttribute is null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            // If we get here a command step trigger was found

            var binding = new CommandStepTriggerBinding(parameter, triggerAttribute);
            return Task.FromResult<ITriggerBinding>(binding);
        }

        public CommandStepTriggerAttributeBindingProvider(IConfiguration configuration)
        {

        }

    }
}
