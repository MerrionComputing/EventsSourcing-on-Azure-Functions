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
    public class NewEntityTriggerBindingProvider
        : ITriggerBindingProvider
    {

        /// <summary>
        /// Create a new trigger binding to trigger a function when a new entity is created
        /// </summary>
        /// <param name="context">
        /// The trigger binding provider context for this binding
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if no trigger binding provider context is passed in
        /// </exception>
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;

            NewEntityTriggerAttribute triggerAttribute =
            parameter.GetCustomAttribute<NewEntityTriggerAttribute>(inherit: false);
            if (triggerAttribute is null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            // If we get here a new entity trigger was found

            var binding = new NewEntityTriggerBinding( parameter, triggerAttribute );
            return Task.FromResult<ITriggerBinding>(binding);
        }

        public NewEntityTriggerBindingProvider(IConfiguration configuration)
        {

        }
    }

}
