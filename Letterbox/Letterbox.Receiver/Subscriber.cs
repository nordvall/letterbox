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
        private string _topicName;
        private string _subscriptionName;
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
            //Console.WriteLine("{0} receiving on thread {1}", _topicName, Thread.CurrentThread.ManagedThreadId);

            SubscriptionClient client = result.AsyncState as SubscriptionClient;
            BrokeredMessage message = client.EndReceive(result);

            if (message != null)
            {
                T inner = message.GetBody<T>();
                _consumer.Consume(inner);

                message.Complete();
            }

            Subscribe();
        }

        public void Unsubscribe()
        {
            _client.Close();
        }
    }
}
