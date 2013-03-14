using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Subscriptions
{
    public interface IConsumer<T> //: IConsumer
    {
        void Consume(T message);
    }

    public interface IConsumer
    {
        void Consume(object message);
    }
}
