using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public ServiceBusClient(Uri serviceBusAdress)
        {
            _webClient = new WebClientWrapper();
            _serviceBusAddress = serviceBusAdress;
        }

        public ServiceBusClient(Uri serviceBusAdress, IWebClient webClient)
        {
            _webClient = webClient;
            _serviceBusAddress = serviceBusAdress;
        }

        public string AccessToken { get; set; }

        public void SubmitToTopic(string topicName, object message)
        {
            Uri topicUri = GenerateTopicUri(topicName);

            byte[] data = SerializeMessage(message);

            ConfigureAuthorizationHeader();
            _webClient.Headers.Add(System.Net.HttpRequestHeader.ContentType, "application/atom+xml;type=entry;charset=utf-8");

            byte[] responseBytes = _webClient.UploadData(topicUri, "POST", data);
            string response = Encoding.UTF8.GetString(responseBytes);
        }

        public string ReceiveFromTopic(string topicName, string subscriptionName)
        {
            Uri subscriptionUri = GenerateSubscriptionUri(topicName, subscriptionName);

            ConfigureAuthorizationHeader();

            byte[] responseBytes = _webClient.UploadData(subscriptionUri, "POST", new byte[0]);
            string response = Encoding.UTF8.GetString(responseBytes);
            return response;
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

        private void ConfigureAuthorizationHeader()
        {
            string tokenValue = string.Format("WRAP access_token=\"{0}\"", AccessToken);
            _webClient.Headers.Add(System.Net.HttpRequestHeader.Authorization, tokenValue);
            
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
