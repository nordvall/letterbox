using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private SenderCache _senders;
        private IClientFactory _clientFactory;
        private IQueueValidator _validator;

        public ServiceBus(IClientFactory clientFactory)
        {
            _subscribers = new List<ISubscriber>();
            _senders = new SenderCache();
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

            subscriber.EnvelopeReceived += delegate(object obj, SubscriberEventArgs args) { DispatchEvent(MessageReceived, args); };
            subscriber.EnvelopeFailed += delegate(object obj, SubscriberEventArgs args) { DispatchEvent(MessageFailed, args); };
            subscriber.EnvelopeConsumed += delegate(object obj, SubscriberEventArgs args) { DispatchEvent(MessageConsumed, args); };

            return subscriber;
        }

        public void SubmitToQueue(string queueName, object message)
        {
            Sender sender = GetQueueSender(queueName);
            sender.Send(message);
        }

        private Sender GetQueueSender(string queueName)
        {
            Sender sender = _senders.GetSender(queueName);

            if (sender == null)
            {
                sender = CreateQueueSender(queueName);
                _senders.AddSender(queueName, sender);
            }

            return sender;
        }

        private Sender CreateQueueSender(string queueName)
        {
            _validator.EnsureQueue(queueName);
            ISendClient client = _clientFactory.CreateQueueClient(queueName);
            Sender sender = new Sender(client);

            return sender;
        }

        public void PublishToTopic(string topicName, object message)
        {
            Sender sender = GetTopicSender(topicName);
            sender.Send(message);
        }

        private Sender GetTopicSender(string topicName)
        {
            Sender sender = _senders.GetSender(topicName);

            if (sender == null)
            {
                sender = CreateTopicSender(topicName);
                _senders.AddSender(topicName, sender);
            }

            return sender;
        }

        private Sender CreateTopicSender(string topicName)
        {
            _validator.EnsureTopic(topicName);
            ISendClient client = _clientFactory.CreateTopicClient(topicName);
            Sender sender = new Sender(client);

            return sender;
        }

        public void Stop()
        {
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Unsubscribe();
            }
        }

        private void DispatchEvent(SubscriberEventHandler handler, SubscriberEventArgs eventArgs)
        {
            if (handler != null)
            {
                handler.Invoke(this, eventArgs);
            }
        }

        public event SubscriberEventHandler MessageReceived;
        public event SubscriberEventHandler MessageConsumed;
        public event SubscriberEventHandler MessageFailed;
    }
}
