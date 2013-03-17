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
        private Stream _messageStream;
        private Uri _messageUri { get; set; }

        public WebClientEnvelope(HttpWebResponse response, ServiceBusClient client)
        {
            _response = response;
            _properties = response.Headers["BrokerProperties"];
            _client = client;

            _messageUri = new Uri(response.Headers["Location"]);

            CopyMessageContents(response);
        }

        
        private void CopyMessageContents(HttpWebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                _messageStream = new MemoryStream();
                int count = 0;

                do
                {
                    byte[] buffer = new byte[1024];
                    count = responseStream.Read(buffer, 0, 1024);
                    _messageStream.Write(buffer, 0, count);
                } while (responseStream.CanRead && count > 0);
            }
        }

        public override T GetMessage<T>() 
        {
            var serializer = new DataContractSerializer(typeof(T));
            _messageStream.Position = 0;

            var quotas = new XmlDictionaryReaderQuotas();
            using (var reader = XmlDictionaryReader.CreateBinaryReader(_messageStream, quotas))
            {
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
            HttpWebRequest request = _client.CreateWebRequest("PUT", _messageUri);
            using (_client.GetResponse(request)) { };
        }

        public override void Complete()
        {
            DeleteMessage();
        }

        private void DeleteMessage()
        {
            HttpWebRequest request = _client.CreateWebRequest("DELETE", _messageUri);
            using (_client.GetResponse(request)) { };
        }
    }
}
