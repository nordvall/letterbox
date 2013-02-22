using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Letterbox.WebClient
{
    public class TokenClient
    {
        private IWebClient _webClient;
        private Uri _serviceBusAddress;

        public TokenClient(Uri serviceBusAdress)
        {
            _webClient = new WebClientWrapper();
            _serviceBusAddress = serviceBusAdress;
        }

        public TokenClient(Uri serviceBusAdress, IWebClient webClient)
        {
            _webClient = webClient;
            _serviceBusAddress = serviceBusAdress;
        }

        private Uri GenerateUri()
        {
            string address = string.Format("{0}://{1}:{2}/{3}/$STS/OAuth/", _serviceBusAddress.Scheme, _serviceBusAddress.DnsSafeHost, _serviceBusAddress.Port, _serviceBusAddress.LocalPath);
            return new Uri(address);
        }

        public string GetAccessToken(string userName, string userPassword)
        {
            //const int ServicePointMaxIdleTimeMilliSeconds = 50000;
            //const string OAuthTokenServicePath = "$STS/OAuth/";
            const string ClientPasswordFormat =
                "grant_type=authorization_code&client_id={0}&client_secret={1}&scope={2}";

            Uri requestUri = GenerateUri();
            string requestContent = string.Format(CultureInfo.InvariantCulture,
                ClientPasswordFormat, HttpUtility.UrlEncode(userName),
                HttpUtility.UrlEncode(userPassword),
                HttpUtility.UrlEncode(_serviceBusAddress.AbsoluteUri));

            byte[] body = Encoding.UTF8.GetBytes(requestContent);

            //HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            //request.ServicePoint.MaxIdleTime = ServicePointMaxIdleTimeMilliSeconds;
            //request.AllowAutoRedirect = true;
            //request.MaximumAutomaticRedirections = 1;
            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = body.Length;
            //request.Timeout = Convert.ToInt32(timeout.TotalMilliseconds,
            //    CultureInfo.InvariantCulture);

            _webClient.Headers.Add("ContentType", "application/x-www-form-urlencoded");
            byte[] responseBytes = _webClient.UploadData(requestUri, "POST", body);

            string rawAccessToken = Encoding.UTF8.GetString(responseBytes); 
            //using (var response = request.GetResponse() as HttpWebResponse)
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        using (var reader = new StreamReader(stream, Encoding.UTF8))
            //        {
            //            rawAccessToken = reader.ReadToEnd();
            //        }
            //    }
            //}

            

            //string simpleWebToken = string.Format(CultureInfo.InvariantCulture,
            //    "WRAP access_token=\"{0}\"", rawAccessToken);
            return rawAccessToken;
        }
    }
}
