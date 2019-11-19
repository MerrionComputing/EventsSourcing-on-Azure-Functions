using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public sealed class ClassificationAttributeBinding
        : IBinding
    {
        private readonly ParameterInfo _parameter;
        private readonly IEventStreamSettings _eventStreamSettings;

        /// <summary>
        /// This binding gets its properties from the Attribute 
        /// </summary>
        public bool FromAttribute
        {
            get { return true; }
        }


        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            return Task.FromResult<IValueProvider>(new ClassificationValueBinder(_parameter,
                _eventStreamSettings));
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            // TODO: Perform any conversions on the incoming value
            return Task.FromResult<IValueProvider>(new ClassificationValueBinder(_parameter,
                _eventStreamSettings));
        }



        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "The classification which this function can run",
                    DefaultValue = "[not applicable]",
                    Prompt = "Please enter a Classification"
                }
            };
        }

        /// <summary>
        /// Create a new binding for the projection
        /// </summary>
        /// <param name="parameter">
        /// Details of the attribute and parameter to be bound to
        /// </param>
        public ClassificationAttributeBinding(ParameterInfo parameter,
            IEventStreamSettings eventStreamSettings)
        {
            _parameter = parameter;
            _eventStreamSettings = eventStreamSettings;
        }
    }
}
