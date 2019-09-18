
using EventSourcingOnAzureFunctions.Common;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class NotificationHelper_UnitTest
    {

        [TestMethod]
        public void MakeEventGridSubjectPart_Unchanged_TestMethod()
        {
            string actual = @"not set";
            string expected = @"ValidSubjectPart";

            actual = NotificationHelper.MakeEventGridSubjectPart(expected);

            Assert.AreEqual(expected, actual); 
        }

        [TestMethod]
        public void MakeEventGridSubjectPart_changed_TestMethod()
        {
            string actual = @"not set";
            string expected = @"Valid/Subject/Part";

            actual = NotificationHelper.MakeEventGridSubjectPart(@"Valid.Subject.Part");

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public async Task NotificationHelper_SendNewEntity_TestMethod()
        {

            IOptions<EventSourcingOnAzureOptions> options = null;
            INameResolver nameResolver = null;

            var config = new ConfigurationBuilder()
                .AddJsonFile("config.local.json")
                .Build();

            EventSourcingOnAzureOptions optionConfig = new EventSourcingOnAzureOptions()
            {
                RaiseEntityCreationNotification= true ,
                EventGridKeyValue = @"set-sas-key",
                EventGridTopicEndpoint = @"https://eventstream-notifications.northeurope-1.eventgrid.azure.net/api/events"
            };

            // make a default name resolver
            nameResolver = new Microsoft.Azure.WebJobs.DefaultNameResolver(config);

            options = Options.Create<EventSourcingOnAzureOptions>(optionConfig);

            NotificationHelper testNotifier = new NotificationHelper(options, nameResolver, null);

            await testNotifier.NewEntityCreated(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 123"));


            Assert.IsNotNull(testNotifier);  
        }
    }
}
