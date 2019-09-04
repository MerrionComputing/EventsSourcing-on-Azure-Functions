using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{

#if TRIGGER_BINDING
    /// <summary>
    /// The provider to bing a NewEntityAttribute to the parameter/function triggered 
    /// when a new entity is created
    /// </summary>
    /// <remarks>
    /// See https://www.tpeczek.com/2018/11/azure-functions-20-extensibility_20.html
    /// </remarks>
    public class NewEntityTriggerBindingProvider
        : ITriggerBindingProvider
    {

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {

            ParameterInfo parameter = context.Parameter;

            NewEntityTriggerAttribute triggerAttribute =
            parameter.GetCustomAttribute<NewEntityTriggerAttribute>(inherit: false);
            if (triggerAttribute is null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

        }

        public NewEntityTriggerBindingProvider(IConfiguration configuration)
        {

        }
    }
#endif
}
