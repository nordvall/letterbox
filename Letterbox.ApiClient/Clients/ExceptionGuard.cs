using Letterbox.Clients;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.ApiClient.Clients
{
    public class ExceptionGuard
    {
        public static void InvokeMethod(Action action)
        {
            try
            {
                action();
            }
            catch (MessagingEntityNotFoundException ex)
            {
                throw new ServiceBusObjectNotFoundException(ex.Message, ex);
            }
            catch (MessagingCommunicationException ex)
            {
                throw new ServiceBusCommunicationException(ex.Message, ex);
            }
        }
    }
}
