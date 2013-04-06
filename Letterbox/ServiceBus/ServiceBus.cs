using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.Subscriptions;

namespace Letterbox.ServiceBus
{
    /// <summary>
    /// Manages all subscribers
    /// </summary>
    public class ServiceBus
    {
        private List<ISubscriber> _subscribers;
        private IClientFactory _clientFactory;

        public ServiceBus(IClientFactory clientFactory)
        {
            _subscribers = new List<ISubscriber>();
            _clientFactory = clientFactory;
        }

        public void Configure(QueueSubscription subscription) 
        {
            IReceiveClient client = _clientFactory.CreateQueueClient(subscription.QueueName);
            CreateAndConfigureSubscriber(client, subscription.Consumer);
        }

        public void Configure(TopicSubscription subscription)
        {
            IReceiveClient client = _clientFactory.CreateSubscriptionClient(subscription.TopicName, subscription.SubscriptionName);
            CreateAndConfigureSubscriber(client, subscription.Consumer);
        }

        private void CreateAndConfigureSubscriber(IReceiveClient client, IConsumer consumer)
        {
            ISubscriber subscriber = new Subscriber(client, consumer);
            
            subscriber.EnvelopeReceived += OnMessageReceived;
            subscriber.EnvelopeFailed += OnMessageFailed;
            subscriber.EnvelopeConsumed += OnMessageConsumed;

            _subscribers.Add(subscriber);
        }

        public void Send(string queueName, object message)
        {
            ISendClient client = _clientFactory.CreateQueueClient(queueName);
            var sender = new Sender(client, message);
            sender.Send();
        }

        public void Publish(string topicName, object message)
        {
            ISendClient client = _clientFactory.CreateTopicClient(topicName);
            var sender = new Sender(client, message);
            sender.Send();
        }

        public void Start()
        {
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Subscribe();
            }
        }

        public void Stop()
        {
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Unsubscribe();
            }
        }

        public event SubscriberEventHandler MessageReceived;

        private void OnMessageReceived(SubscriberEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(e);
            }
        }

        public event SubscriberEventHandler MessageConsumed;

        private void OnMessageConsumed(SubscriberEventArgs e)
        {
            if (MessageConsumed != null)
            {
                MessageConsumed(e);
            }
        }

        public event SubscriberEventHandler MessageFailed;

        private void OnMessageFailed(SubscriberEventArgs e)
        {
            if (MessageFailed != null)
            {
                MessageFailed(e);
            }
        }
    }
}
