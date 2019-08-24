using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using RetailBank.AzureFunctionApp;

[assembly: FunctionsStartup(typeof(AzureFunctionAppStartup))]
namespace RetailBank.AzureFunctionApp
{


    /// <summary>
    /// Start-up class to load all the dependency injection and startup configuration code
    /// </summary>
    public class AzureFunctionAppStartup
        : EventSourcingOnAzureFunctions.Common.Startup
    {


    }

}
