
using EventSourcingOnAzureFunctions.Common;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class NotificationHelper_UnitTest
    {

        [TestInitialize ]
        public void NotificationHelper_UnitTest_Initialise()
        {
            Environment.SetEnvironmentVariable("Bank.Account", "Table;RetailBank"); 
        }

        [TestMethod]
        public void MakeEventGridSubjectPart_Unchanged_TestMethod()
        {
            string actual = @"not set";
            string expected = @"ValidSubjectPart";

            actual = EventGridNotificationDispatcher.MakeEventGridSubjectPart(expected);

            Assert.AreEqual(expected, actual); 
        }

        [TestMethod]
        public void MakeEventGridSubjectPart_changed_TestMethod()
        {
            string actual = @"not set";
            string expected = @"Valid/Subject/Part";

            actual = EventGridNotificationDispatcher.MakeEventGridSubjectPart(@"Valid.Subject.Part");

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
                EventGridKeyValue = @"",
                EventGridTopicEndpoint = @"https://eventstream-notifications.northeurope-1.eventgrid.azure.net/api/events"
            };

            // make a default name resolver
            nameResolver = new Microsoft.Azure.WebJobs.DefaultNameResolver(config);

            options = Options.Create<EventSourcingOnAzureOptions>(optionConfig);

            EventGridNotificationDispatcher testNotifier = new EventGridNotificationDispatcher(options, nameResolver, null);

            await testNotifier.NewEntityCreated(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 1234"));


            Assert.IsNotNull(testNotifier);  
        }

        [TestMethod]
        public async Task NotificationHelper_SendNewEvent_TestMethod()
        {

            IOptions<EventSourcingOnAzureOptions> options = null;
            INameResolver nameResolver = null;

            var config = new ConfigurationBuilder()
                .AddJsonFile("config.local.json")
                .Build();

            EventSourcingOnAzureOptions optionConfig = new EventSourcingOnAzureOptions()
            {
                RaiseEventNotification = true,
                EventGridKeyValue = @"",
                EventGridTopicEndpoint = @"https://eventstream-notifications.northeurope-1.eventgrid.azure.net/api/events"
            };

            // make a default name resolver
            nameResolver = new Microsoft.Azure.WebJobs.DefaultNameResolver(config);

            options = Options.Create<EventSourcingOnAzureOptions>(optionConfig);

            EventGridNotificationDispatcher testNotifier = new EventGridNotificationDispatcher(options, nameResolver, null);

            await testNotifier.NewEventAppended(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 1234"),
                "Event Happened",
                2023);


            Assert.IsNotNull(testNotifier);
        }
    }
}
