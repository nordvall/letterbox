using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Common
{
    public interface IConsumer<T>
    {
        void Consume(T message);
    }
}
