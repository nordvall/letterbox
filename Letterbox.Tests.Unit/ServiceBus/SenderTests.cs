using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.ServiceBus;
using NSubstitute;
using NUnit.Framework;
using System.Threading;

namespace Letterbox.Tests.Unit.ServiceBus
{
    [TestFixture]
    public class SenderTests
    {
        [Test]
        public void Send_WhenCalled_ClientIsInvoked()
        {
            // Arrange
            string message = string.Empty;
            var sender = TestableSender.Create();

            // Act
            sender.Send(message);
            sender.WaitForEvent(SenderEventArgs.SenderEventType.Succeeded, 1);

            // Assert
            sender.StubClient.Received().Send(message);
        }

        //[Test]
        //public void Send_Synchronous_WhenExceptionOccursOnLastAttempt_ExceptionIsForwarded()
        //{
        //    ISendClient client = Substitute.For<ISendClient>();
        //    client.When(c => c.Send(Arg.Any<object>())).Do(c => { throw new Exception(); });

        //    string message = string.Empty;
        //    var sender = new Sender(client, message);

        //    TestDelegate codeToRun = new TestDelegate(() => sender.Send());
        //    Assert.Throws<Exception>(codeToRun);
        //}

        [Test]
        public void Send_WhenFirstAttemptFails_SecondAttemptIsMade()
        {
            int counter = 0;
            var sender = TestableSender.Create();
            sender.StubClient
                .When(c => c.Send(Arg.Any<object>()))
                .Do(c => { 
                    counter++;

                    if (counter < 2)
                    {
                        throw new Exception();
                    }
                });

            string message = string.Empty;
            
            sender.Send(message);

            sender.WaitForEvent(SenderEventArgs.SenderEventType.Succeeded, 1);
            
            //Assert
            sender.StubClient.Received(2).Send(message);
        }
    }
}
