using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;
using System;

[assembly: FunctionsStartup(typeof(EventSourcingOnAzureFunctions.Common.Startup))]
namespace EventSourcingOnAzureFunctions.Common
{
    public class Startup
        : IWebJobsStartup
    {

        public void Configure(IFunctionsHostBuilder builder)
        {

            builder.AddAppSettingsToConfiguration();

            // Initialise any common services
            CQRSAzureBindings.InitializeServices(builder.Services);

        }

        public void Configure(IWebJobsBuilder builder)
        {
            // Add the standard (built-in) bindings 
            builder.AddBuiltInBindings();

            // Allow the execution context to be accessed by DI
            builder.AddExecutionContextBinding();

            //Register any extensions for bindings
            builder.AddExtension<InjectConfiguration>();
        }

    }

    /// <summary>
    /// Dependency injection configuration for this Azure functions app
    /// </summary>
    public class InjectConfiguration
        : IExtensionConfigProvider
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly IHostingEnvironment _hostingEnvironment;

        public InjectConfiguration(IServiceProvider serviceProvider,
            IHostingEnvironment hostingEnvironment)
        {
            _serviceProvider = serviceProvider;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            // Add any custom bindings for this domain
            // and then add any core ones from CQRSAzureBindings.Common
            CQRSAzureBindings.InitializeInjectionConfiguration(context );

             
        }
    }
}
