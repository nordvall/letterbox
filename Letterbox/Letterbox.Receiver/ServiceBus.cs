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
    }
}
