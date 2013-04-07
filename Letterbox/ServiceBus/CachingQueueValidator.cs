using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;

namespace Letterbox.ServiceBus
{
    public class CachingQueueValidator : IQueueValidator
    {
        private HashSet<string> _validQueues = new HashSet<string>();
        private Dictionary<string, HashSet<string>> _validTopics = new Dictionary<string,HashSet<string>>();
        private IQueueValidator _innerValidator;

        public CachingQueueValidator(IQueueValidator innerValidator)
        {
            _innerValidator = innerValidator;
        }

        public void EnsureQueue(string queueName)
        {
            if (_validQueues.Contains(queueName))
            {
                return;
            }

            lock (_validQueues)
            {
                if (_validQueues.Contains(queueName))
                {
                    return;
                }

                _innerValidator.EnsureQueue(queueName);
                _validQueues.Add(queueName);
            }
        }

        public void EnsureTopic(string topicName)
        {
            if (_validTopics.ContainsKey(topicName))
            {
                return;
            }

            lock (_validTopics)
            {
                if (_validTopics.ContainsKey(topicName))
                {
                    return;
                }

                _innerValidator.EnsureTopic(topicName);
                _validTopics.Add(topicName, new HashSet<string>());
            }
        }

        public void EnsureSubscription(string topicName, string subscriptionName)
        {
            EnsureTopic(topicName);

            if (_validTopics[topicName].Contains(subscriptionName))
            {
                return;
            }

            lock (_validTopics)
            {
                if (_validTopics[topicName].Contains(subscriptionName))
                {
                    return;
                }

                _innerValidator.EnsureSubscription(topicName, subscriptionName);
                _validTopics[topicName].Add(subscriptionName);
            }
        }
    }
}
