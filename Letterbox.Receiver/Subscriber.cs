using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Common;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.Receiver
{
    public class Subscriber<T> : ISubscriber
    {
        private IConsumer<T> _consumer;
        private SubscriptionClient _client;

        public Subscriber(SubscriptionClient client, IConsumer<T> consumer)
        {
            _consumer = consumer;
            _client = client;
        }


        public void Subscribe()
        {
            _client.BeginReceive(new TimeSpan(0, 0, 15), new AsyncCallback(MessageArrived), _client);
        }

        private void MessageArrived(IAsyncResult result)
        {
            SubscriptionClient client = result.AsyncState as SubscriptionClient;
            BrokeredMessage message = null;

            try
            {
                message = client.EndReceive(result);

                if (message != null)
                {
                    T inner = message.GetBody<T>();
                    _consumer.Consume(inner);

                    message.Complete();

                    if (MessageConsumed != null)
                    {
                        var args = new SubscriberEventArgs()
                        {
                            EventType = SubscriberEventArgs.SubscriberEventType.Consumed,
                            TopicName = _client.TopicPath,
                            SubscriptionName = _client.Name
                        };

                        MessageConsumed(this, args);
                    }
                }
            }
            catch(Exception ex)
            {
                if (message != null)
                {
                    if (MessageFailed != null)
                    {
                        var args = new SubscriberEventArgs()
                        {
                            EventType = SubscriberEventArgs.SubscriberEventType.Failed,
                            Message = ex.Message,
                            TopicName = _client.TopicPath,
                            SubscriptionName = _client.Name
                        };

                        MessageFailed(this, args);
                    }
                    message.DeadLetter();
                    //message.Abandon();
                }
            }

            Subscribe();
        }

        public void Unsubscribe()
        {
            _client.Close();
        }

        public event SubscriberEventHandler MessageReceived;
        public event SubscriberEventHandler MessageConsumed;
        public event SubscriberEventHandler MessageFailed;
    }

    
}
