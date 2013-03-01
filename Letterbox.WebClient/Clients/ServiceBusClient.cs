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
    public class ServiceBusClient : ReceiveClientBase, ISendReceiveClient
    {
        private Uri _address;
        private ITokenManager _tokenManager;
        private IWebClient _webClient;

        public ServiceBusClient(Uri address, ITokenManager tokenManager)
        {
            _address = address;
            _tokenManager = tokenManager;
            _webClient = new WebClientWrapper();
        }

        public ServiceBusClient(Uri address, ITokenManager tokenManager, IWebClient webClient)
        {
            _address = address;
            _tokenManager = tokenManager;
            _webClient = webClient;

            Timeout = 15;
        }

        public void Send(object message)
        {
            var url = new Uri(_address, string.Format("{0}/messages", _address.AbsolutePath));
            byte[] data = SerializeMessage(message);
            HttpWebRequest request = CreateWebRequest(url, data);
            _webClient.SendRequest(request);
        }

        public override Envelope Receive()
        {
            var url = new Uri(_address, string.Format("{0}/messages/head?timeout={1}", _address.AbsolutePath, Timeout));
            HttpWebRequest request = CreateWebRequest(url, new byte[0]);
            HttpWebResponse response = _webClient.SendRequest(request);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                Envelope envelope = new WebClientEnvelope(response);
                return envelope;
            }
            return null;
        }

        //private HttpWebRequest CreateSendRequest(byte[] data)
        //{
        //    var url = new Uri(_address, string.Format("{0}/messages", _address.AbsolutePath));
        //    HttpWebRequest request = CreateWebRequest(url, data);
        //    return request;
        //}

        //private HttpWebRequest CreateReceiveRequest()
        //{
        //    var url = new Uri(_address, string.Format("{0}/messages/head?timeout={1}", _address.AbsolutePath, Timeout));
        //    HttpWebRequest request = CreateWebRequest(url, new byte[0]);
        //    return request;
        //}

        public void DeadLetter(Guid lockTooken)
        {
            throw new NotImplementedException();
        }

        public void Defer(Guid lockTooken)
        {
            throw new NotImplementedException();
        }

        public void Abandon(Guid lockTooken)
        {
            throw new NotImplementedException();
        }

        public void Complete(Guid lockTooken)
        {
            var url = new Uri(_address, string.Format("{0}/messages/head?timeout={1}", _address.AbsolutePath, Timeout));
            HttpWebRequest request = CreateWebRequest(url, new byte[0]);
            HttpWebResponse response = _webClient.SendRequest(request);
            
        }

        public void Close()
        {

        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        protected HttpWebRequest CreateWebRequest(Uri requestUri, byte[] data)
        {
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 1;
            request.Method = "POST";
            request.ContentType = "application/atom+xml;type=entry;charset=utf-8";
            request.ContentLength = data.Length;

            AccessToken token = _tokenManager.GetAccessToken();
            string tokenHeaderValue = string.Format("WRAP access_token=\"{0}\"", token.TokenValue);
            request.Headers.Add(HttpRequestHeader.Authorization, tokenHeaderValue);

            if (data.Length > 0)
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                }
            }

            return request;
        }

        protected byte[] SerializeMessage(object message)
        {
            MemoryStream stream = new MemoryStream();

            var serializer = new DataContractSerializer(message.GetType());

            var writer = XmlDictionaryWriter.CreateBinaryWriter(stream);
            serializer.WriteObject(writer, message);
            writer.Flush();

            return stream.ToArray();
        }


        public int Timeout
        {
            get;
            set;
        }
    }
}
