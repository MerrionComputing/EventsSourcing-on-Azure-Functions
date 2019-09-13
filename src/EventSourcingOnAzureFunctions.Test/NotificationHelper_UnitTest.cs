
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    }
}
