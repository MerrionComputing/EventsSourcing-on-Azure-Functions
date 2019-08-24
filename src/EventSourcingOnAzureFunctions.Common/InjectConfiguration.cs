using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Hosting;
using System;

namespace EventSourcingOnAzureFunctions.Common
{
    /// <summary>
    /// Dependency injection configuration for this Azure functions app
    /// </summary>
    [Extension("EventSourcing")]
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
            CQRSAzureBindings.InitializeInjectionConfiguration(context);


        }
    }
}
