using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Letterbox.Clients;

namespace Letterbox.WebClient.Clients
{
    public class WebClientEnvelope : Envelope
    {
        private HttpWebResponse _response;
        private string _properties;

        public WebClientEnvelope(HttpWebResponse response)
        {
            _response = response;
            _properties = response.Headers["BrokerProperties"];
        }

        public override T GetMessage<T>() 
        {
            var serializer = new DataContractSerializer(typeof(T));

            using (Stream stream = _response.GetResponseStream())
            {
                var quotas = new XmlDictionaryReaderQuotas();
                var reader = XmlDictionaryReader.CreateBinaryReader(stream, quotas);
                T message = serializer.ReadObject(reader, false) as T;
                return message;
            }
        }
    }
}
