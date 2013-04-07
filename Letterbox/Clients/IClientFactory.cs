using System;
using Letterbox.Clients;

namespace Letterbox.Clients
{
    public interface IClientFactory
    {
        ISendReceiveClient CreateQueueClient(string queueName);
        IReceiveClient CreateSubscriptionClient(string topicName, string subscriptionName);
        ISendClient CreateTopicClient(string topicName);
        IQueueValidator GetValidator();
    }
}
