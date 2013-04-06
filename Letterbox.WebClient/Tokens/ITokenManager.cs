using System;
using System.Net;

namespace Letterbox.WebClient.Tokens
{
    public interface ITokenManager
    {
        AccessToken GetAccessToken();
    }
}
