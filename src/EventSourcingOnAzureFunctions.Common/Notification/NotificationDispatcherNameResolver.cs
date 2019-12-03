using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    public class NotificationDispatcherNameResolver
        : INameResolver
    {

        private readonly IConfiguration _configuration;

        public NotificationDispatcherNameResolver(IConfiguration config)
        {
            _configuration = config;
        }

        public string Resolve(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return _configuration[name];
        }
    }
}
