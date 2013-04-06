using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.ServiceBus;
using NSubstitute;
using NUnit.Framework;

namespace Letterbox.Tests.ServiceBus
{
    [TestFixture]
    public class SenderTests
    {
        [Test]
        public void Send_Synchronous_WhenCalled_ClientIsInvoked()
        {
            ISendClient client = Substitute.For<ISendClient>();
            string message = string.Empty;
            var sender = new Sender(client, message);
            sender.Synchronous = true;
            sender.Send();

            client.Received().Send(message);
        }

        [Test]
        public void Send_Synchronous_WhenExceptionOccursOnLastAttempt_ExceptionIsForwarded()
        {
            ISendClient client = Substitute.For<ISendClient>();
            client.When(c => c.Send(Arg.Any<object>())).Do(c => { throw new Exception(); });

            string message = string.Empty;
            var sender = new Sender(client, message);
            sender.Synchronous = true;
            sender.MaxAttempts = 1;

            TestDelegate codeToRun = new TestDelegate(() => sender.Send());
            Assert.Throws<Exception>(codeToRun);
        }

        [Test]
        public void Send_Synchronous_WhenFirstAttemptFails_SecondAttemptIsMade()
        {
            int counter = 0;
            ISendClient client = Substitute.For<ISendClient>();
            client
                .When(c => c.Send(Arg.Any<object>()))
                .Do(c => { 
                    counter++;

                    if (counter < 2)
                    {
                        throw new Exception();
                    }
                });

            string message = string.Empty;
            var sender = new Sender(client, message);
            sender.Synchronous = true;
            sender.MaxAttempts = 2;

            //Act
            sender.Send();

            //Assert
            client.Received(2).Send(message);
        }
    }
}
