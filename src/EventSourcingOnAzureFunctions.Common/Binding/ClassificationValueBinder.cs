using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public sealed class ClassificationValueBinder
                : IValueBinder
    {

        private readonly ParameterInfo _parameter;
        private readonly IEventStreamSettings _eventStreamSettings;

        public Type Type
        {
            get
            {
                return typeof(Classification );
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
                ClassificationAttribute  attribute = _parameter.GetCustomAttribute<ClassificationAttribute >(inherit: false);
                if (null != attribute)
                {
                    item = new Classification (attribute,
                        settings: _eventStreamSettings);
                }
            }

            return item;
        }

        /// <summary>
        /// This will be expanded out to make sure the domain, aggregate and classification type really exist,
        /// and are mapped
        /// </summary>
        /// <param name="parameter">
        /// The classification parameter
        /// </param>
        private Task ValidateParameter(ParameterInfo parameter)
        {
            return Task.CompletedTask;
        }

        public ClassificationValueBinder(ParameterInfo parameter,
            IEventStreamSettings eventStreamSettings)
        {
            _parameter = parameter;
            _eventStreamSettings = eventStreamSettings;
        }
    }
}
