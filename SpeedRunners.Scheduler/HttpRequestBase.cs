using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpeedRunners.Scheduler
{
    public static class HttpRequestBase
    {
        public static HttpClient CreateHttpClient()
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
            string proxyAddress = ConfigurationManager.AppSettings["ProxyAddress"];
            var proxy = new WebProxy(proxyAddress, false);
            HttpClient.DefaultProxy = proxy;
            return client;
        }

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly LogHelper Log = LogHelper.GetCurrentClassLogHelper();

        public static async Task<string> HttpGetAsync(this HttpClient client, string url)
        {
            string result = null;
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Log.Log(DateTime.Now + ex.ToString());
            }
            return result;
        }

    }
}
