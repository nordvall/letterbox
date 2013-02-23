using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Letterbox.WebClient
{
    public class ServiceBusClient
    {
        private IWebClient _webClient;
        private Uri _serviceBusAddress;
        private ITokenManager _tokenManager;

        public ServiceBusClient(Uri serviceBusAdress, ITokenManager tokenManager)
        {
            _webClient = new WebClientWrapper();
            _serviceBusAddress = serviceBusAdress;
            _tokenManager = tokenManager;
        }

        public ServiceBusClient(Uri serviceBusAdress, ITokenManager tokenManager, IWebClient webClient)
        {
            _webClient = webClient;
            _serviceBusAddress = serviceBusAdress;
            _tokenManager = new TokenManager(serviceBusAdress, _webClient);
        }

        public void SubmitToTopic(string topicName, object message)
        {
            Uri topicUri = GenerateTopicUri(topicName);

            byte[] data = SerializeMessage(message);

            HttpWebRequest request = CreateWebRequest(topicUri, data);

            _webClient.SendRequest(request);
        }

        public string ReceiveFromTopic(string topicName, string subscriptionName)
        {
            Uri subscriptionUri = GenerateSubscriptionUri(topicName, subscriptionName);

            HttpWebRequest request = CreateWebRequest(subscriptionUri, new byte[0]);

            string response = _webClient.SendRequest(request);
            return response;
        }


        private HttpWebRequest CreateWebRequest(Uri requestUri, byte[] data)
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

        private byte[] SerializeMessage(object message)
        {
            MemoryStream stream = new MemoryStream();

            var serializer = new DataContractSerializer(message.GetType());

            var writer = XmlDictionaryWriter.CreateBinaryWriter(stream);
            serializer.WriteObject(writer, message);
            writer.Flush();

            return stream.ToArray();
        }
        
        public void SubmitToQueue(string queueName, object message)
        {

        }

        private Uri GenerateTopicUri(string topicName)
        {
            string address = string.Format("{0}://{1}:{2}{3}/{4}/messages", _serviceBusAddress.Scheme, _serviceBusAddress.DnsSafeHost, _serviceBusAddress.Port, _serviceBusAddress.LocalPath, topicName);
            return new Uri(address);
        }

        private Uri GenerateSubscriptionUri(string topicName, string subscriptionName)
        {
            string address = string.Format("{0}://{1}:{2}{3}/{4}/Subscriptions/{5}/messages/head?timeout=10", _serviceBusAddress.Scheme, _serviceBusAddress.DnsSafeHost, _serviceBusAddress.Port, _serviceBusAddress.LocalPath, topicName, subscriptionName);
            return new Uri(address);
        }
    }
}
