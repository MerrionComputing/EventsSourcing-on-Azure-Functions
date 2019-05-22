using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public sealed class ProjectionValueBinder
         : IValueBinder
    {

        private readonly ParameterInfo _parameter;

        public Type Type
        {
            get
            {
                return typeof(Projection);
            }
        }



        public string ToInvokeString()
        {
            return string.Empty;
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetValueAsync()
        {

            object item = null;

            await ValidateParameter(_parameter);

            if (null != _parameter)
            {
                ProjectionAttribute attribute = _parameter.GetCustomAttribute<ProjectionAttribute>(inherit: false);
                if (null != attribute)
                {
                    item = new Projection(attribute);
                }
            }

            return item;
        }

        /// <summary>
        /// This will be expanded out to make sure the domain, aggregate and projection type really exist,
        /// and are mapped
        /// </summary>
        /// <param name="parameter">
        /// The Projection parameter
        /// </param>
        /// <returns></returns>
        private Task ValidateParameter(ParameterInfo parameter)
        {
            return Task.CompletedTask;
        }

        public ProjectionValueBinder(ParameterInfo parameter)
        {
            _parameter = parameter;
        }
    }
}
