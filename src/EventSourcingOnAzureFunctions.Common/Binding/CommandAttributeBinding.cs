using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public sealed class CommandAttributeBinding
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


        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "The command to be executed by this function",
                    DefaultValue = "[not applicable]",
                    Prompt = "Please enter a Command definition"
                }
            };
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            return Task.FromResult<IValueProvider>(new CommandValueBinder(_parameter));
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            // TODO: Perform any conversions on the incoming value
            return Task.FromResult<IValueProvider>(new CommandValueBinder(_parameter));
        }

        public CommandAttributeBinding(ParameterInfo parameter)
        {
            _parameter = parameter;
        }
    }

}
