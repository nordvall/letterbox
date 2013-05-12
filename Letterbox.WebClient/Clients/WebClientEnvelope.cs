using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Letterbox.Clients;
using Letterbox.WebClient.Tokens;
using Letterbox.WebClient.Web;

namespace Letterbox.WebClient.Clients
{
    public class WebClientEnvelope : Envelope, IDisposable
    {
        private HttpWebResponse _response;
        private string _properties;
        private IWebClient _client;
        private Stream _messageStream;
        private Uri _messageUri { get; set; }
        private WebRequestFactory _webRequestFactory;
        private MessageSerializer _serializer;
        private IWebTokenProvider _tokenManager;

        public WebClientEnvelope(HttpWebResponse response)
        {
            _response = response;
            _properties = response.Headers["BrokerProperties"];
            _serializer = new MessageSerializer();

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
            T message = _serializer.DeserializeMessage<T>(_messageStream);
            return message;
        }

        public void Dispose()
        {
            _messageStream.Dispose();
        }
    }
}
