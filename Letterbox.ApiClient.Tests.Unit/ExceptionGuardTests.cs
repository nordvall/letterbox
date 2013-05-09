using Letterbox.ApiClient.Clients;
using Letterbox.Clients;
using Microsoft.ServiceBus.Messaging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Letterbox.ApiClient.Tests.Unit
{
    [TestFixture]
    public class ExceptionGuardTests
    {
        [Test]
        public void InvokeMethod_WhenActionThrowsMessagingCommunicationException_ServiceBusCommunicationExceptionIsReturned()
        {
            TestDelegate codeToRun = () => { ExceptionGuard.InvokeMethod(() => { throw new MessagingCommunicationException("path"); }); };
            ServiceBusCommunicationException ex = Assert.Throws<ServiceBusCommunicationException>(codeToRun);
            Assert.AreEqual(typeof(MessagingCommunicationException), ex.InnerException.GetType());
        }

        [Test]
        public void InvokeMethod_WhenActionThrowsMessagingEntityNotFoundException_ServiceBusObjectNotFoundExceptionIsReturned()
        {
            TestDelegate codeToRun = () => { ExceptionGuard.InvokeMethod(() => { throw new MessagingEntityNotFoundException("path"); }); };
            ServiceBusObjectNotFoundException ex = Assert.Throws<ServiceBusObjectNotFoundException>(codeToRun);
            Assert.AreEqual(typeof(MessagingEntityNotFoundException), ex.InnerException.GetType());
        }
    }
}
