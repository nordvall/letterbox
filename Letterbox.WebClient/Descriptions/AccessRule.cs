using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.WebClient.Clients
{
    public class AccessRule
    {
        public AccessRule()
        {
            Permissions = new List<AccessRight>();
        }

        public string ServiceBusNamespace { get; set; }
        public string UserName { get; set; }
        public List<AccessRight> Permissions { get; set; }
    }
}
