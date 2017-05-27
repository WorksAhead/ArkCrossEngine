using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Security;
using System.IO;

namespace ServerBridge
{
  internal static class HttpWebUtility
  {
    /// <summary>  
    /// 创建POST方式的HTTP请求  
    /// </summary>  
    /// <param name="url">请求的URL</param>  
    /// <param name="parameters">HTTP POST请求头</param> 
    /// <param name="parameters">HTTP POST请求参数</param>  
    /// <param name="timeout">请求的超时时间</param>  
    /// <returns></returns>  
    internal static HttpWebRequest CreatePostHttpRequest(string url, IDictionary<string, string> httpHeaders, IDictionary<string, string> parameters, int timeout)
    {
      if (string.IsNullOrEmpty(url)) {
        throw new ArgumentNullException("url");
      }
      HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded";
      request.Timeout = timeout;
      request.ReadWriteTimeout = timeout;

      //这2句看来没啥用，先留着
      request.SendChunked = false;
      request.TransferEncoding = null;
      
      //下面2句必须启用一个，否则有一定概率接收响应失败。。（可能与billing的处理有关系）
      request.KeepAlive = false;
      //request.ProtocolVersion = new Version(1, 0);

      //http请求头
      if (!(httpHeaders == null || httpHeaders.Count == 0)) {
        foreach (var item in httpHeaders) {
          request.Headers.Set(item.Key, item.Value);
        }
      }
      //Post参数
      if (!(parameters == null || parameters.Count == 0)) {
        StringBuilder buffer = new StringBuilder();
        int i = 0;
        foreach (string key in parameters.Keys) {
          if (i > 0) {
            buffer.AppendFormat("&{0}={1}", key, parameters[key]);
          } else {
            buffer.AppendFormat("{0}={1}", key, parameters[key]);
          }
          i++;
        }
        byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
        request.ContentLength = data.Length;
        using (Stream stream = request.GetRequestStream()) {
          stream.Write(data, 0, data.Length);
        }
      }
      return request;
    }
  }
}
