using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public class EventStreamValueBinder
         : IValueBinder
    {

        private readonly ParameterInfo _parameter;
        private readonly IEventStreamSettings _eventStreamSettings;
        private EventStream _item;

        public Type Type
        {
            get
            {
                return _parameter.ParameterType;
            }
        }


        /// <summary>
        /// Turn the parameter (with attribute) into an EventStream object
        /// </summary>
        /// <returns>
        /// A built [EventStream] object
        /// </returns>
        public Task<object> GetValueAsync()
        {

            if (null == _item)
            {
                if (null != _parameter)
                {
                    EventStreamAttribute attribute = _parameter.GetCustomAttribute<EventStreamAttribute>(inherit: false);
                    if (null != attribute)
                    {
                        _item = new EventStream(attribute,
                            settings:_eventStreamSettings  );
                    }
                }
            }

            return Task.FromResult<object>(_item);
        }


        public string ToInvokeString()
        {
            if (null != _item)
            {
                return _item.ToString();
            }
            return string.Empty;
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        /// <summary>
        /// This will be expanded out to make sure the domain and aggregate really exist,
        /// and are mapped
        /// </summary>
        /// <param name="parameter">
        /// The EventStream parameter
        /// </param>
        /// <returns></returns>
        private Task ValidateParameter(ParameterInfo parameter)
        {
            return Task.CompletedTask;
        }

        public EventStreamValueBinder(ParameterInfo parameter,
            IEventStreamSettings eventStreamSettings)
        {
            _parameter = parameter;
            _eventStreamSettings = eventStreamSettings;
        }
    }
}
