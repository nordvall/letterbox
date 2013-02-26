using System;
using Letterbox.Clients;
using Letterbox.Common.Subscriptions;

namespace Letterbox.Receiver.Clients
{
    public interface IClientFactory
    {
        ISendReceiveClient CreateQueueClient(string queueName);
        IReceiveClient CreateSubscriptionClient(string topicName, string subscriptionName);
        ISendClient CreateTopicClient(string topicName);
    }
}
