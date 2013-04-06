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
    public class WebClientEnvelope : Envelope
    {
        private HttpWebResponse _response;
        private string _properties;
        private IWebClient _client;
        private Stream _messageStream;
        private Uri _messageUri { get; set; }
        private WebRequestFactory _webRequestFactory;
        private MessageSerializer _serializer;

        public WebClientEnvelope(HttpWebResponse response, IWebClient client, ITokenManager tokenManager)
        {
            _response = response;
            _properties = response.Headers["BrokerProperties"];
            _client = client;
            _webRequestFactory = new WebRequestFactory(tokenManager);
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
            HttpWebRequest request = _webRequestFactory.CreateWebRequest("PUT", _messageUri);
            SendWebRequest(request);
        }

        public override void Complete()
        {
            DeleteMessage();
        }

        private void DeleteMessage()
        {
            HttpWebRequest request = _webRequestFactory.CreateWebRequest("DELETE", _messageUri);
            SendWebRequest(request);
        }

        private void SendWebRequest(HttpWebRequest request)
        {
            HttpWebResponse response = null;

            try
            {
                response = _client.SendRequest(request);
            }
            catch (Exception)
            {
                // TODO: Translate exceptions.
                throw;
            }
            finally
            {
                // TODO: Something to dispose?
            }
        }
    }
}
