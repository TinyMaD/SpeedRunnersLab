using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpeedRunners.Utils
{
    public static class HttpHelper
    {
        public static async Task<string> HttpGet(string url, CookieContainer cookie = null)
        {
            HttpWebResponse response = null;
            string result = null;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            IWebProxy noProxy = request.Proxy;
            request.Timeout = 10000;
            request.Method = "GET";
            request.KeepAlive = false;
            if (cookie != null)
            {
                request.CookieContainer = cookie;
            }
            try
            {
                var enable = AppSettings.GetConfig("Proxy:Enable");
                if (enable == "true")
                {

                    WebProxy proxyObject = new WebProxy("localhost", 7890);//str为IP地址 port为端口号
                    request.Proxy = proxyObject; //设置代理 
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
            catch (Exception)
            {
                try
                {
                    request.Proxy = noProxy;
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
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            IWebProxy noProxy = req.Proxy;
            req.Method = "post";
            req.Timeout = 10000;
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = reqbytes.Length;
            try
            {
                var enable = AppSettings.GetConfig("Proxy:Enable");
                if (enable == "true")
                {
                    WebProxy proxyObject = new WebProxy("localhost", 7890);//str为IP地址 port为端口号
                    req.Proxy = proxyObject; //设置代理 
                }

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
            catch (Exception)
            {
                try
                {
                    req.Proxy = noProxy;
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
            }
            return PageStr;
        }

        public static void SetProxy()
        {
            var enable = AppSettings.GetConfig("Proxy:Enable");
            if (enable != "true") return;

            //1.设置带用户和密码的代理
            var Address = AppSettings.GetConfig("Proxy:Address");//地址
            //var Account = ConfigCommon.Configuration["Proxy:Account"];//用户名
            //var Password = ConfigCommon.Configuration["Proxy:Password"];//密码
            var webProxy = new WebProxy(Address, false);
            webProxy.BypassProxyOnLocal = true;

            HttpClient.DefaultProxy = webProxy;
        }
    }
}