using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ServiceBus(Uri queueUri, Uri stsUri)
        {
            _subscribers = new List<ISubscriber>();
            _clientFactory = new AzureClientFactory(queueUri, stsUri);
        }

        public void Configure<T>(QueueSubscription<T> subscription)
        {
            IClient client = _clientFactory.CreateQueueClient(subscription);
            CreateAndConfigureSubscriber<T>(client, subscription.Consumer);
        }

        public void Configure<T>(TopicSubscription<T> subscription)
        {
            IClient client = _clientFactory.CreateTopicClient(subscription);
            CreateAndConfigureSubscriber<T>(client, subscription.Consumer);
        }

        private void CreateAndConfigureSubscriber<T>(IClient client, IConsumer<T> consumer)
        {
            ISubscriber subscriber = new Subscriber<T>(client, consumer);
            
            subscriber.MessageReceived += OnMessageReceived;
            subscriber.MessageFailed += OnMessageFailed;
            subscriber.MessageConsumed += OnMessageConsumed;

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

        private void OnMessageReceived(ISubscriber sender, SubscriberEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }
        }

        public event SubscriberEventHandler MessageConsumed;

        private void OnMessageConsumed(ISubscriber sender, SubscriberEventArgs e)
        {
            if (MessageConsumed != null)
            {
                MessageConsumed(sender, e);
            }
        }

        public event SubscriberEventHandler MessageFailed;

        private void OnMessageFailed(ISubscriber sender, SubscriberEventArgs e)
        {
            if (MessageFailed != null)
            {
                MessageFailed(sender, e);
            }
        }
    }
}
