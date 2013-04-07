using System;
namespace Letterbox.Clients
{
    public interface IQueueValidator
    {
        void EnsureQueue(string queueName);
        void EnsureSubscription(string topicName, string subscriptionName);
        void EnsureTopic(string topicName);
    }
}
