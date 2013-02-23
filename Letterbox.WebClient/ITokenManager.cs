using System;
namespace Letterbox.WebClient
{
    public interface ITokenManager
    {
        AccessToken GetAccessToken();
    }
}
