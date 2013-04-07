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
        private IQueueValidator _validator;

        public ServiceBus(IClientFactory clientFactory)
        {
            _subscribers = new List<ISubscriber>();
            _clientFactory = clientFactory;
            
            IQueueValidator innerValidator = clientFactory.GetValidator();
            _validator = new CachingQueueValidator(innerValidator);
        }

        public void AddConsumer(QueueSubscription subscription) 
        {
            _validator.EnsureQueue(subscription.QueueName);

            IReceiveClient client = _clientFactory.CreateQueueClient(subscription.QueueName);
            ISubscriber subscriber = CreateAndConfigureSubscriber(client, subscription.Consumer);
            subscriber.Subscribe();
            _subscribers.Add(subscriber);
        }

        public void AddConsumer(TopicSubscription subscription)
        {
            _validator.EnsureSubscription(subscription.TopicName, subscription.SubscriptionName);

            IReceiveClient client = _clientFactory.CreateSubscriptionClient(subscription.TopicName, subscription.SubscriptionName);
            ISubscriber subscriber = CreateAndConfigureSubscriber(client, subscription.Consumer);
            subscriber.Subscribe(); 
            _subscribers.Add(subscriber);
        }

        private ISubscriber CreateAndConfigureSubscriber(IReceiveClient client, IConsumer consumer)
        {
            ISubscriber subscriber = new Subscriber(client, consumer);
            
            subscriber.EnvelopeReceived += OnMessageReceived;
            subscriber.EnvelopeFailed += OnMessageFailed;
            subscriber.EnvelopeConsumed += OnMessageConsumed;

            return subscriber;
        }

        public void SendToQueue(string queueName, object message)
        {
            _validator.EnsureQueue(queueName);

            ISendClient client = _clientFactory.CreateQueueClient(queueName);
            var sender = new Sender(client, message);
            sender.Synchronous = true;
            sender.Send();
        }

        public void PublishToTopic(string topicName, object message)
        {
            _validator.EnsureTopic(topicName);

            ISendClient client = _clientFactory.CreateTopicClient(topicName);
            var sender = new Sender(client, message);
            sender.Send();
        }

        public void Stop()
        {
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Unsubscribe();
            }
        }

        public event SubscriberEventHandler MessageReceived;

        private void OnMessageReceived(object sender, SubscriberEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }
        }

        public event SubscriberEventHandler MessageConsumed;

        private void OnMessageConsumed(object sender, SubscriberEventArgs e)
        {
            if (MessageConsumed != null)
            {
                MessageConsumed(sender, e);
            }
        }

        public event SubscriberEventHandler MessageFailed;

        private void OnMessageFailed(object sender, SubscriberEventArgs e)
        {
            if (MessageFailed != null)
            {
                MessageFailed(sender, e);
            }
        }
    }
}
