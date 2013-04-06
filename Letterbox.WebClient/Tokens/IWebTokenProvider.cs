using System;
using System.Net;

namespace Letterbox.WebClient.Tokens
{
    public interface IWebTokenProvider
    {
        WebToken GetAccessToken();
    }
}
