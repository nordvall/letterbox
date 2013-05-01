﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.ServiceBus
{
    public class SenderEventArgs : EventArgs
    {
        public SenderEventType EventType { get; set; }
        public SenderEnvelope Envelope { get; set; }
        public string ErrorMessage { get; set; }

        public enum SenderEventType
        {
            Attempted,
            Succeeded,
            Failed
        }
    }
}
