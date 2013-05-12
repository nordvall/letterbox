using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.Subscriptions;

namespace Letterbox.ServiceBus
{
    /// <summary>
    /// Uses an IClient to poll for messages for one specific queue/topic. 
    /// Calls IConsumer for each message received.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Subscriber : ISubscriber
    {
        private IConsumer _consumer;
        private IReceiveClient _client;

        public Subscriber(IReceiveClient client, IConsumer consumer)
        {
            _consumer = consumer;
            _client = client;
        }

        public string Name
        {
            get { return _client.Name; }
        }


        public void Subscribe()
        {
            _client.BeginReceive(new AsyncCallback(MessageArrived));
        }

        private void MessageArrived(IAsyncResult result)
        {
            Envelope envelope = null;

            try
            {
                envelope = _client.EndReceive(result);

                if (envelope != null)
                {
                    var receivedEventArgs = new SubscriberEventArgs(envelope, SubscriberEventArgs.SubscriberEventType.Received);
                    DispatchEvent(EnvelopeReceived, receivedEventArgs);


                    _consumer.ConsumeEnvelope(envelope);

                    _client.Complete(envelope);

                    var completedEventArgs = new SubscriberEventArgs(envelope, SubscriberEventArgs.SubscriberEventType.Consumed);
                    DispatchEvent(EnvelopeConsumed, completedEventArgs);

                }
            }
            catch(TimeoutException)
            { 
                // just reconnect
            }
            catch(Exception ex)
            {
                var eventArgs = new SubscriberEventArgs(envelope, SubscriberEventArgs.SubscriberEventType.Failed);
                eventArgs.ErrorMessage = ex.Message;
                DispatchEvent(EnvelopeFailed, eventArgs);
            }

            Subscribe();
        }

        public void Unsubscribe()
        {
            _client.Close();
        }

        private void DispatchEvent(SubscriberEventHandler handler, SubscriberEventArgs eventArgs)
        {
            if (handler != null)
            {
                handler.Invoke(this, eventArgs);
            }
        }

        public event SubscriberEventHandler EnvelopeReceived;
        public event SubscriberEventHandler EnvelopeConsumed;
        public event SubscriberEventHandler EnvelopeFailed;
    }
}
