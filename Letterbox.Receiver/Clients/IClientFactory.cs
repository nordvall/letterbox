using System;
using Letterbox.Common.Subscriptions;

namespace Letterbox.Receiver.Clients
{
    /// <summary>
    /// Creates IClient:s for queue and topic subscriptions
    /// </summary>
    public interface IClientFactory
    {
        IClient CreateClient<T>(QueueSubscription<T> subscription);
        IClient CreateClient<T>(TopicSubscription<T> subscription);
    }
}
