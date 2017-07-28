using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Utils
{
    class HttpUtils
    {
        public static string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "application/json; charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public static void HttpPut(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "PUT";
            request.ContentType = "application/json; charset=UTF-8";
            byte[] bytes = Encoding.UTF8.GetBytes(postDataStr);

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            //return retString;
        }
      
        public static string HttpPost(string url, string postDataStr)
        {          
            WebRequest webRequest = WebRequest.Create(url);
            HttpWebRequest httpRequest = webRequest as HttpWebRequest;
            if (httpRequest == null)
            {
                throw new ApplicationException(string.Format("Invalid url string: {0}", url));
            }

            // 填充httpWebRequest的基本信息
            //httpRequest.UserAgent = sUserAgent;
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Method = "POST";

            Encoding encoding = Encoding.GetEncoding("utf-8");
            byte[] data = encoding.GetBytes(postDataStr);
            // 填充要post的内容
            httpRequest.ContentLength = data.Length;
            Stream requestStream = httpRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            // 发送post请求到服务器并读取服务器返回信息
            Stream responseStream = httpRequest.GetResponse().GetResponseStream();

            // 读取服务器返回信息
            string stringResponse = string.Empty;
            using (StreamReader responseReader =new StreamReader(responseStream, Encoding.GetEncoding("utf-8")))
            {
                stringResponse = responseReader.ReadToEnd();
            }
            responseStream.Close();

            return stringResponse;
        }
    }
}
