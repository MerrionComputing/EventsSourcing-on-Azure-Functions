using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    public sealed class QueryValueBinder
        : IValueBinder
    {

        private readonly ParameterInfo _parameter;


        public Type Type
        {
            get
            {
                return typeof(Query);
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
                QueryAttribute attribute = _parameter.GetCustomAttribute<QueryAttribute>(inherit: false);
                if (null != attribute)
                {
                    item = new Query(attribute);
                }
            }

            return item;
        }


        private Task ValidateParameter(ParameterInfo parameter)
        {
            return Task.CompletedTask;
        }


        public QueryValueBinder(ParameterInfo parameter)
        {
            _parameter = parameter;
        }
    }
}
