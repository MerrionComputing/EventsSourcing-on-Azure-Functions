using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<EventSourcingOnAzureOptions> _options; 

        public InjectConfiguration(IServiceProvider serviceProvider,
            IHostingEnvironment hostingEnvironment,
            IOptions<EventSourcingOnAzureOptions> options)
        {
            _serviceProvider = serviceProvider;
            _hostingEnvironment = hostingEnvironment;
            _options = options;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            // Add any custom bindings for this domain
            // and then add any core ones from CQRSAzureBindings.Common
            CQRSAzureBindings.InitializeInjectionConfiguration(context);


        }
    }
}
