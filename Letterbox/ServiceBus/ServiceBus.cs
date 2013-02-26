using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.Common;
using Letterbox.Common.Subscriptions;
using Letterbox.Receiver.Clients;
using Letterbox.Receiver.Subscriptions;

namespace Letterbox.Receiver
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

        public void Configure<T>(QueueSubscription<T> subscription) where T : class, new()
        {
            IReceiveClient client = _clientFactory.CreateQueueClient(subscription.QueueName);
            CreateAndConfigureSubscriber<T>(client, subscription.Consumer);
        }

        public void Configure<T>(TopicSubscription<T> subscription) where T : class, new()
        {
            IReceiveClient client = _clientFactory.CreateSubscriptionClient(subscription.TopicName, subscription.SubscriptionName);
            CreateAndConfigureSubscriber<T>(client, subscription.Consumer);
        }

        private void CreateAndConfigureSubscriber<T>(IReceiveClient client, IConsumer<T> consumer) where T : class, new()
        {
            ISubscriber subscriber = new Subscriber<T>(client, consumer);
            
            subscriber.EnvelopeReceived += OnMessageReceived;
            subscriber.EnvelopeFailed += OnMessageFailed;
            subscriber.EnvelopeConsumed += OnMessageConsumed;

            _subscribers.Add(subscriber);
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
