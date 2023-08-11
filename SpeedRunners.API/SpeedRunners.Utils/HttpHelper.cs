using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpeedRunners.Utils
{
    public class HttpHelper
    {
        public static async Task<string> HttpGet(string url, CookieContainer cookie = null)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            string result = null;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 10000;
                request.Method = "GET";
                request.KeepAlive = false;
                if (cookie != null)
                {
                    request.CookieContainer = cookie;
                }
                //request.ContentType = "application/x-www-form-urlencoded";
                response = await request.GetResponseAsync() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                if (stream != null)
                {
                    StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                    result = sr.ReadToEnd();
                    stream.Close();
                    stream = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (request != null)
                    request.Abort();
                if (response != null)
                    response.Close();
            }
            return result;
        }
        public static async Task<string> HttpPost(string TheURL, string data)
        {
            string PageStr = string.Empty;
            Uri url = new Uri(TheURL);
            byte[] reqbytes = Encoding.UTF8.GetBytes(data);
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "post";
                req.Timeout = 10000;
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = reqbytes.Length;
                Stream stm = req.GetRequestStream();
                stm.Write(reqbytes, 0, reqbytes.Length);

                stm.Close();
                HttpWebResponse wr = await req.GetResponseAsync() as HttpWebResponse;
                Stream stream = wr.GetResponseStream();
                StreamReader srd = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
                PageStr += srd.ReadToEnd();
                stream.Close();
                srd.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PageStr;
        }
    }
}
