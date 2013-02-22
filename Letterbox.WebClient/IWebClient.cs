using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Letterbox.WebClient
{
    public interface IWebClient
    {
        WebHeaderCollection Headers { get; set; }

        byte[] UploadData(Uri address, string method, byte[] data);
    }
}
