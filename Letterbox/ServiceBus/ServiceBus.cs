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
        private Dictionary<string, Sender> _senders;
        private ReaderWriterLockSlim _senderCacheLock = new ReaderWriterLockSlim();
        private IClientFactory _clientFactory;
        private IQueueValidator _validator;

        public ServiceBus(IClientFactory clientFactory)
        {
            _subscribers = new List<ISubscriber>();
            _senders = new Dictionary<string, Sender>();
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

        private Sender LookupSenderInCache(string name)
        {
            if (_senders.ContainsKey(name))
            {
                return _senders[name];
            }
            else
            {
                return null;
            }
        }

        public void SubmitToQueue(string queueName, object message)
        {
            _senderCacheLock.EnterReadLock();
            Sender sender = LookupSenderInCache(queueName);
            _senderCacheLock.ExitReadLock();

            if (sender == null)
            {
                _senderCacheLock.EnterWriteLock();

                try
                {
                    sender = LookupSenderInCache(queueName);

                    if (sender == null)
                    {
                        _validator.EnsureQueue(queueName);

                        ISendClient client = _clientFactory.CreateQueueClient(queueName);
                        sender = new Sender(client);
                        _senders.Add(queueName, sender);
                    }
                }
                finally
                {
                    _senderCacheLock.ExitWriteLock();
                }
                
            }
            
            sender.Send(message);
        }

        public void PublishToTopic(string topicName, object message)
        {
            _senderCacheLock.EnterReadLock();
            Sender sender = LookupSenderInCache(topicName);
            _senderCacheLock.ExitReadLock();

            if (sender == null)
            {
                _senderCacheLock.EnterWriteLock();

                try
                {
                    sender = LookupSenderInCache(topicName);
                    if (sender == null)
                    {
                        _validator.EnsureTopic(topicName);

                        ISendClient client = _clientFactory.CreateTopicClient(topicName);
                        sender = new Sender(client);
                        _senders.Add(topicName, sender);
                    }
                }
                finally
                {
                    _senderCacheLock.ExitWriteLock();
                }
            }

            sender.Send(message);
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
