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
    public class ServiceBusClient : ISendReceiveClient
    {
        private Uri _address;
        private IWebTokenProvider _tokenManager;
        private IWebClient _webClient;
        private WebRequestFactory _webRequestFactory;
        private MessageSerializer _serializer;

        public ServiceBusClient(Uri address, IWebTokenProvider tokenManager)
            : this(address, tokenManager, new WebClientWrapper())
        { }

        public ServiceBusClient(Uri address, IWebTokenProvider tokenManager, IWebClient webClient)
        {
            _address = address;
            _tokenManager = tokenManager;
            _webClient = webClient;
            _webRequestFactory = new WebRequestFactory(tokenManager);
            _serializer = new MessageSerializer();
        }

        public ushort Timeout { get; set; }

        public IAsyncResult BeginSend(object message, AsyncCallback callback)
        {
            Action<object> sendMethod = Send;
            return sendMethod.BeginInvoke(message, callback, sendMethod);
        }

        public void Send(object message)
        {
            var url = new Uri(_address, string.Format("{0}/messages", _address.AbsolutePath));
            byte[] data = _serializer.SerializeMessage(message);
            HttpWebRequest request = _webRequestFactory.CreateWebRequest("POST", url, data);
            _webClient.SendRequest(request);
        }

        public void EndSend(IAsyncResult result)
        {
            Action<object> sendMethod = result.AsyncState as Action<object>;
            sendMethod.EndInvoke(result);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback)
        {
            Func<Envelope> receiveMethod = Receive;
            return receiveMethod.BeginInvoke(callback, receiveMethod);
        }

        public Envelope Receive()
        {
            var url = new Uri(_address, string.Format("{0}/messages/head?timeout={1}", _address.AbsolutePath, Timeout));
            HttpWebRequest request = _webRequestFactory.CreateWebRequest("POST", url, new byte[0]);
           
            using (HttpWebResponse response = _webClient.SendRequest(request))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Envelope envelope = new WebClientEnvelope(response, _webClient, _tokenManager);
                    return envelope;
                }
            }
            return null;
        }

        public Envelope EndReceive(IAsyncResult result)
        {
            Func<Envelope> receiveMethod = result.AsyncState as Func<Envelope>;
            return receiveMethod.EndInvoke(result);
        }

        public void Close()
        {

        }

        public string Name
        {
            get { return _address.AbsolutePath; }
        }
    }
}
