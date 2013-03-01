using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public abstract class ReceiveClientBase
    {
        private static int counter = 0;
        public int objectNumber;

        public ReceiveClientBase()
        {
            Timeout = 15;

            counter++;
            objectNumber = counter;
        }

        public int Timeout { get; set; }

        public abstract Envelope Receive();

        public IAsyncResult BeginReceive(AsyncCallback callback)
        {
            Func<Envelope> receiveMethod = Receive;
            return receiveMethod.BeginInvoke(callback, receiveMethod);
        }

        public Envelope EndReceive(IAsyncResult result)
        {
            Func<Envelope> receiveMethod = result.AsyncState as Func<Envelope>;
            return receiveMethod.EndInvoke(result);
        }
    }
}
