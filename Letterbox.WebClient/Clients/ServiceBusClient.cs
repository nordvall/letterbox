﻿using System;
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

        public void Send(object message)
        {
            var url = new Uri(_address, string.Format("{0}/messages", _address.AbsolutePath));
            byte[] data = _serializer.SerializeMessage(message);
            HttpWebRequest request = _webRequestFactory.CreateWebRequestWithData("POST", url, data);
            _webClient.SendRequest(request);
        }

        public Envelope Receive()
        {
            var url = new Uri(_address, string.Format("{0}/messages/head?timeout={1}", _address.AbsolutePath, Timeout));
            HttpWebRequest request = _webRequestFactory.CreateWebRequest("POST", url);
           
            using (HttpWebResponse response = _webClient.SendRequest(request))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Envelope envelope = new WebClientEnvelope(response);
                    return envelope;
                }
            }
            return null;
        }

        public void Complete(Envelope envelope)
        {
            var url = new Uri(_address, string.Format("{0}/messages/{1}/{2}", _address.AbsolutePath, envelope.MessageId, envelope.LockToken));
            HttpWebRequest request = _webRequestFactory.CreateWebRequest("DELETE", url);

            using (HttpWebResponse response = _webClient.SendRequest(request))
            {

            }

            var resource = envelope as IDisposable;
            if (resource != null)
            {
                resource.Dispose();
            }
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
