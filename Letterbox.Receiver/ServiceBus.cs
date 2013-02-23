using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Receiver
{
    public class ServiceBus
    {
        private List<ISubscriber> _subscribers;

        public ServiceBus()
        {
            _subscribers = new List<ISubscriber>();
        }

        public void Configure<T>(Subscription<T> subscription)
        {
            ISubscriber subscriber = SubscriberFactory.CreateSubscription(subscription);
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

        private void OnMessageReceived(ISubscriber sender, SubscriberEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }
        }

        private void OnMessageConsumed(ISubscriber sender, SubscriberEventArgs e)
        {
            if (MessageConsumed != null)
            {
                MessageConsumed(sender, e);
            }
        }

        private void OnMessageFailed(ISubscriber sender, SubscriberEventArgs e)
        {
            if (MessageFailed != null)
            {
                MessageFailed(sender, e);
            }
        }

        public event SubscriberEventHandler MessageReceived;
        public event SubscriberEventHandler MessageConsumed;
        public event SubscriberEventHandler MessageFailed;
    }
}
