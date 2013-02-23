using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.WebClient
{
    public class AccessToken
    {
        private readonly DateTime baseDate = new DateTime(1970, 1, 1);

        public AccessToken(string tokenValue)
        {
            TokenValue = tokenValue;
            ParseToken();
        }

        private void ParseToken()
        {
            string[] tokenParts = TokenValue.Split('&');

            foreach (string tokenPart in tokenParts)
            {
                if (!string.IsNullOrEmpty(tokenPart))
                {
                    string[] keyValue = tokenPart.Split('=');
                    if (keyValue.Length == 2)
                    {
                        switch (keyValue[0])
                        {
                            case "ExpiresOn":
                                Int64 value = Int64.Parse(keyValue[1]);
                                Exprires = baseDate.AddSeconds(value);
                                break;
                            case "Audience":
                                Audience = keyValue[1];
                                break;
                            case "Upn":
                                UserName = keyValue[1];
                                break;
                        }
                    }
                }
            }
        }

        public string TokenValue { get; private set; }
        public string Audience { get; private set; }
        public string UserName { get; private set; }
        public DateTime Exprires { get; private set; }
    }
}
