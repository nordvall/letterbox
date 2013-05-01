using Letterbox.Clients;
using Letterbox.ServiceBus;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Letterbox.Tests.Unit.ServiceBus
{
    public class TestableSender : Sender
    {
        private Synchronizer _messageSucceededSynchronizer;
        private Synchronizer _messageFailedSynchronizer;
        private Synchronizer _messageAttemptedSynchronizer;
        
        private ISendClient _client;

        public TestableSender(ISendClient client)
            : base(client)
        {
            _client = client;

            SetupSynchronizationEventHandlers();
        }

        private void SetupSynchronizationEventHandlers()
        {
            _messageAttemptedSynchronizer = new Synchronizer();
            MessageAttempted += new SenderEventHandler(new Action<object, SenderEventArgs>((obj, e) =>
            {
                _messageAttemptedSynchronizer.Pulse();
            }));

            _messageFailedSynchronizer = new Synchronizer();
            MessageFailed += new SenderEventHandler(new Action<object, SenderEventArgs>((obj, e) =>
            {
                _messageFailedSynchronizer.Pulse();
            }));

            _messageSucceededSynchronizer = new Synchronizer();
            MessageSucceeded += new SenderEventHandler(new Action<object, SenderEventArgs>((obj, e) =>
            {
                _messageSucceededSynchronizer.Pulse();
            }));
        }

        public ISendClient StubClient
        {
            get { return _client; }
        }

        public static TestableSender Create()
        {
            ISendClient client = CreateStubClient();
            var sender = new TestableSender(client);

            return sender;
        }

        private static ISendClient CreateStubClient()
        {
            ISendClient client = Substitute.For<ISendClient>();
            client.BeginSend(Arg.Any<object>(), Arg.Any<AsyncCallback>()).Returns(x =>
            {
                Action<object> sendMethod = client.Send;
                return sendMethod.BeginInvoke(x.Args()[0], x.Args()[1] as AsyncCallback, sendMethod);
            });

            client.When(x => x.EndSend(Arg.Any<IAsyncResult>())).Do(x =>
            {
                IAsyncResult result = x.Args()[0] as IAsyncResult;
                Action<object> sendMethod = result.AsyncState as Action<object>;
                sendMethod.EndInvoke(result);
            });

            return client;
        }

        public void WaitForEvent(SenderEventArgs.SenderEventType eventType, ushort numberOfTimes)
        {
            switch (eventType)
            {
                case SenderEventArgs.SenderEventType.Attempted:
                    _messageAttemptedSynchronizer.Wait(numberOfTimes);
                    break;
                case SenderEventArgs.SenderEventType.Succeeded:
                    _messageSucceededSynchronizer.Wait(numberOfTimes);
                    break;
                case SenderEventArgs.SenderEventType.Failed:
                    _messageFailedSynchronizer.Wait(numberOfTimes);
                    break;
            }
        }
    }
}
