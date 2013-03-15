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
        private ServiceBusClient _client;

        public WebClientEnvelope(HttpWebResponse response, ServiceBusClient client)
        {
            _response = response;
            _properties = response.Headers["BrokerProperties"];
            _client = client;

            MessageUri = new Uri(response.Headers["Location"]);
        }

        public Uri MessageUri { get; set; }

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

        public override void DeadLetter()
        {
            UnlockMessage();
        }

        public override void Defer()
        {
            UnlockMessage();
        }

        public override void Abandon()
        {
            UnlockMessage();
        }

        private void UnlockMessage()
        {
            HttpWebRequest request = _client.CreateWebRequest("PUT", MessageUri);
            using (_client.GetResponse(request)) { };
        }

        public override void Complete()
        {
            DeleteMessage();
        }

        private void DeleteMessage()
        {
            HttpWebRequest request = _client.CreateWebRequest("DELETE", MessageUri);
            using (_client.GetResponse(request)) { };
        }
    }
}
