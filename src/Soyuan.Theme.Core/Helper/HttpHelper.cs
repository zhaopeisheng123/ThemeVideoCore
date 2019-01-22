using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

namespace Soyuan.Theme.Core.Helper
{
    public class HttpHelper
    {
        public static string HttpPost(string data, string url)
        {
            try
            {
                HttpContent httpVideoContent = new StringContent(data);
                httpVideoContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var client = new HttpClient();
                var response = client.PostAsync(url, httpVideoContent).Result.Content.ReadAsStringAsync().Result;
                return response;
            }
            catch (System.Exception e)
            {
                LogHelper.logError("Post调取接口异常：" + e.ToString());
                throw e;
            }
        }

        public static string HttpDelete(string url)
        {
            try
            {
                var client = new HttpClient();
                var response = client.DeleteAsync(url).Result.Content.ReadAsStringAsync().Result;
                return response;
            }
            catch (System.Exception e)
            {
                LogHelper.logError("Post调取接口异常：" + e.ToString());
                throw e;
            }
        }

        public static string HttpPut(string data, string url)
        {
            try
            {
                HttpContent httpVideoContent = new StringContent(data);
                httpVideoContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var client = new HttpClient();
                var response = client.PutAsync(url, httpVideoContent).Result.Content.ReadAsStringAsync().Result;
                return response;
            }
            catch (System.Exception e)
            {
                LogHelper.logError("Put调取接口异常：" + e.ToString());
                throw e;
            }
        }

        public static string httpGet(string url)
        {
            try
            {
                var client = new HttpClient();
                var response = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                return response;
            }
            catch (Exception e)
            {
                LogHelper.logError("Get调取接口异常：" + e.ToString());
                throw e;
            }

        }

        /// <summary> 
        /// 采用Socket方式，测试服务器连接 
        /// </summary> 
        /// <param name="host">服务器主机名或IP</param> 
        /// <param name="port">端口号</param> 
        /// <param name="millisecondsTimeout">等待时间：毫秒</param> 
        /// <returns></returns> 
        public static bool TestConnection(string host, int port, int millisecondsTimeout = 5)
        {
            TcpClient client = new TcpClient();
            try
            {
                var ar = client.BeginConnect(host, port, null, null);
                ar.AsyncWaitHandle.WaitOne(millisecondsTimeout);
                return client.Connected;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                client.Close();
            }
        }
    }
}
