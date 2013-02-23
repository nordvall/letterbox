using System;
using Letterbox.Common.Subscriptions;

namespace Letterbox.Receiver.Clients
{
    /// <summary>
    /// Creates IClient:s for queue and topic subscriptions
    /// </summary>
    public interface IClientFactory
    {
        IClient CreateQueueClient<T>(QueueSubscription<T> subscription);
        IClient CreateTopicClient<T>(TopicSubscription<T> subscription);
    }
}
