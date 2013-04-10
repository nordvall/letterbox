using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Letterbox.ApiClient.Tests.Integration.ServiceBus
{
    public static class TestUsers
    {
        public static NetworkCredential User1 = new NetworkCredential("servicebususer1", "servicebususer1");
        public static NetworkCredential User2 = new NetworkCredential("servicebususer2", "servicebususer2");
        public static NetworkCredential User3 = new NetworkCredential("servicebususer3", "servicebususer3");
    }
}
