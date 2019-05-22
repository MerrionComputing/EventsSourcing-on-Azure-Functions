using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public class EventStreamAttributeBinding
        : IBinding
    {

        private readonly ParameterInfo _parameter;

        /// <summary>
        /// This binding gets its properties from the Attribute 
        /// </summary>
        public bool FromAttribute
        {
            get { return true; }
        }



        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            return Task.FromResult<IValueProvider>(new EventStreamValueBinder(_parameter));
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            // TODO: Perform any conversions on the incoming value
            return Task.FromResult<IValueProvider>(new EventStreamValueBinder(_parameter));
        }



        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "The Event Stream to which this function can append events",
                    DefaultValue = "[not applicable]",
                    Prompt = "Please enter an Event Stream"
                }
            };
        }

        /// <summary>
        /// Create a new binding for the event stream
        /// </summary>
        /// <param name="parameter">
        /// Details of the attribute and parameter to be bound to
        /// </param>
        public EventStreamAttributeBinding(ParameterInfo parameter)
        {
            _parameter = parameter;
        }
    }
}
