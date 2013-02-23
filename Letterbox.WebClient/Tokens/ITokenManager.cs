using System;

namespace Letterbox.WebClient.Tokens
{
    public interface ITokenManager
    {
        AccessToken GetAccessToken();
    }
}
