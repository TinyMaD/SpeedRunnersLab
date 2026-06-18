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
            IWebProxy proxy = GetConfiguredProxy();
            try
            {
                return await DoHttpGet(url, cookie, proxy);
            }
            catch (WebException ex) when (proxy != null && ex.Response == null)
            {
                // 代理链路失败（无服务端响应）时直连重试一次；
                // 服务端已明确响应的错误（如 403）直接抛出，保留原始信息供调用方判断
                return await DoHttpGet(url, cookie, null);
            }
        }

        private static async Task<string> DoHttpGet(string url, CookieContainer cookie, IWebProxy proxy)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 10000;
            request.Method = "GET";
            request.KeepAlive = false;
            if (cookie != null)
            {
                request.CookieContainer = cookie;
            }
            if (proxy != null)
            {
                request.Proxy = proxy;
            }
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        public static async Task<string> HttpPost(string TheURL, string data)
        {
            IWebProxy proxy = GetConfiguredProxy();
            try
            {
                return await DoHttpPost(TheURL, data, proxy);
            }
            catch (WebException ex) when (proxy != null && ex.Response == null)
            {
                return await DoHttpPost(TheURL, data, null);
            }
        }

        private static async Task<string> DoHttpPost(string url, string data, IWebProxy proxy)
        {
            byte[] reqbytes = Encoding.UTF8.GetBytes(data);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.Timeout = 10000;
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = reqbytes.Length;
            if (proxy != null)
            {
                req.Proxy = proxy;
            }
            using (Stream stm = await req.GetRequestStreamAsync())
            {
                stm.Write(reqbytes, 0, reqbytes.Length);
            }
            using (HttpWebResponse wr = (HttpWebResponse)await req.GetResponseAsync())
            using (Stream stream = wr.GetResponseStream())
            using (StreamReader srd = new StreamReader(stream, Encoding.UTF8))
            {
                return srd.ReadToEnd();
            }
        }

        private static IWebProxy GetConfiguredProxy()
        {
            if (AppSettings.GetConfig("Proxy:Enable") != "true")
            {
                return null;
            }
            var address = AppSettings.GetConfig("Proxy:Address");
            return new WebProxy(address, false);
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
