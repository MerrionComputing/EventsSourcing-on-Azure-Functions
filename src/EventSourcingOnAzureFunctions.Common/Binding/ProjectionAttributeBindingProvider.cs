using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// Output binding provider to select the projection runner to run for a function
    /// </summary>
    public sealed class ProjectionAttributeBindingProvider
        : IBindingProvider
    {

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Determine whether we should bind to the current parameter
            ParameterInfo parameter = context.Parameter;
            ProjectionAttribute attribute = parameter.GetCustomAttribute<ProjectionAttribute>(inherit: false);

            if (attribute == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            // What data type(s) can this attribute be attached to?
            IEnumerable<Type> supportedTypes = new Type[] { typeof(Projection) };

            if (!(parameter.ParameterType == typeof(Projection)))
            {
                throw new InvalidOperationException(
                    $"Can't bind ProjectionAttribute to type '{parameter.ParameterType}'.");
            }

            return Task.FromResult<IBinding>(new ProjectionAttributeBinding(parameter));

        }
    }
}
