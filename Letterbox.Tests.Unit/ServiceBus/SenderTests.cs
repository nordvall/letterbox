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

        [Test]
        public void Send_WhenSendIsAttempted_CorrectEventIsFired()
        {
            var sender = TestableSender.Create();
            string message = string.Empty;
            sender.Send(message);
            sender.WaitForEvent(SenderEventArgs.SenderEventType.Attempted, 1);

            Assert.Pass();
        }

        [Test]
        public void Send_WhenSendSucceeds_CorrectEventIsFired()
        {
            var sender = TestableSender.Create();
            string message = string.Empty;
            sender.Send(message);
            sender.WaitForEvent(SenderEventArgs.SenderEventType.Succeeded, 1);
            Assert.Pass();
        }

        [Test]
        public void Send_WhenSendFails_CorrectEventIsFired()
        {
            var sender = TestableSender.Create();
            sender.StubClient
                .When(c => c.Send(Arg.Any<object>()))
                .Do(c =>
                {
                    throw new Exception();
                });

            string message = string.Empty;
            sender.Send(message);
            sender.WaitForEvent(SenderEventArgs.SenderEventType.Failed, 1);

            Assert.Pass();
        }

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

        [Test]
        public void Send_WhenMessageFails_StatusIsUpdatedCorrectly()
        {
            int counter = 0;
            var sender = TestableSender.Create();
            sender.StubClient
                .When(c => c.Send(Arg.Any<object>()))
                .Do(c =>
                {
                    counter++;

                    if (counter < 2)
                    {
                        throw new Exception();
                    }
                });

            string message = string.Empty;

            Assert.AreEqual(Sender.SenderStatus.Idle, sender.Status);

            sender.Send(message);

            sender.WaitForEvent(SenderEventArgs.SenderEventType.Failed, 1);
            Assert.AreEqual(Sender.SenderStatus.WaitForRetry, sender.Status);

            sender.WaitForEvent(SenderEventArgs.SenderEventType.Succeeded, 1);
            Assert.AreEqual(Sender.SenderStatus.Idle, sender.Status);

        }

        [Test]
        public void Send_WhenFirstMessageFails_FollowingMessagesAreQueued()
        {
            int counter = 0;
            var sender = TestableSender.Create();
            sender.StubClient
                .When(c => c.Send(Arg.Any<object>()))
                .Do(c =>
                {
                    counter++;

                    if (counter < 2)
                    {
                        throw new Exception();
                    }
                });

            string message = string.Empty;

            sender.Send(message); // Should fail and start the retry timer
            sender.Send(message); // Should be placed in queue until retry timer expires
            sender.Send(message); // Should also be queued

            sender.WaitForEvent(SenderEventArgs.SenderEventType.Succeeded, 3);

            //Assert
            sender.StubClient.Received(4).Send(message);
        }
    }
}
