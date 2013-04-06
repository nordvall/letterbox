using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Letterbox.Clients;
using Microsoft.ServiceBus;

namespace Letterbox.ApiClient.Tokens
{
    public class ApiTokenProviderFactory
    {
        private Uri _stsUri;
        private NetworkCredential _credential;

        public ApiTokenProviderFactory(Uri httpsUri)
            : this(httpsUri, null)
        { }

        public ApiTokenProviderFactory(Uri httpsUri, NetworkCredential credential)
        {
            _stsUri = httpsUri;
            _credential = credential;
        }

        public TokenProvider GetTokenProvider()
        {
            Uri[] stsUris = new Uri[] { _stsUri };

            TokenProvider provider = null;

            if (_credential == null)
            {
                provider = TokenProvider.CreateWindowsTokenProvider(stsUris);
            }
            else
            {
                provider = TokenProvider.CreateOAuthTokenProvider(stsUris, _credential);
            }

            return provider;
        }
    }
}
