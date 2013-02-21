using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Receiver
{
    public interface ISubscriber
    {
        void Subscribe();
        void Unsubscribe();
    }
}
