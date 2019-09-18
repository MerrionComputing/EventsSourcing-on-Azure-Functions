using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    public static class ServiceCollectionFactory
    {
        public static IServiceCollection ServiceProvider { get; }

        static ServiceCollectionFactory()
        {
            HostingEnvironment env = new HostingEnvironment();
            env.ContentRootPath = Directory.GetCurrentDirectory();
            env.EnvironmentName = "Development";

            IFunctionsHostBuilder fhn = null;
            Startup startup = new Startup();
            // Add all the default services for this application
            startup.Configure(fhn);
            ServiceProvider = fhn.Services;
        }
    }
}
