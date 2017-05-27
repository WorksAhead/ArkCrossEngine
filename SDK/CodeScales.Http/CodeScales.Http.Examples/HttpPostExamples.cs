using System;
using System.Collections.Generic;
using System.Text;

using CodeScales.Http;
using CodeScales.Http.Methods;
using CodeScales.Http.Entity;
using CodeScales.Http.Common;

namespace CodeScales.Http.Examples
{
    public class HttpPostExamples
    {
        public static void DoPost()
        {
          /*
          HttpClient client = new HttpClient();
          HttpPost postMethod = new HttpPost(new Uri("http://www.w3schools.com/asp/demo_simpleform.asp"));

          List<NameValuePair> nameValuePairList = new List<NameValuePair>();
          nameValuePairList.Add(new NameValuePair("fname", "brian"));
            
          UrlEncodedFormEntity formEntity = new UrlEncodedFormEntity(nameValuePairList, Encoding.UTF8);
          postMethod.Entity = formEntity;
            
          HttpResponse response = client.Execute(postMethod);

          Console.WriteLine("Response Code: " + response.ResponseCode);
          Console.WriteLine("Response Content: " + EntityUtils.ToString(response.Entity));
          */

          HttpClient client = new HttpClient();
          client.Timeout = 10000;
          HttpPost postMethod = new HttpPost(new Uri("http://tmobilebilling.changyou.com/billing"));

          postMethod.Headers.Add("appkey", "1407921103977");
          postMethod.Headers.Add("sign", "51e912635a5b5350");
          postMethod.Headers.Add("tag", "7911376");
          postMethod.Headers.Add("opcode", "10001");
          postMethod.Headers.Add("channelId", "4001");

          List<NameValuePair> nameValuePairList = new List<NameValuePair>();
          nameValuePairList.Add(new NameValuePair("data", "{\"validateInfo\":\"50d2f67deb4b7ba881c130ee401997b098ff53568703b5c4af095530e98f79dc5d273a851db2d6208aa1bbc5873f7b731e3999cb6bf6607544bf8c2cbb5dc114838857023a62ca07c4eb355fb4b1fa9482e22a55b3a5ce8c2b678bfe24627963719442d2ecf2a8a89e95b28ec819f4dd5bad82be017b13b5051dc7387f5570ae182f60962c4b54cba4b4eb9c83e8027cc08cb4281921ebda3fbf4d8bb14a85767a449420e07b72c86eb584ee1b1b88516e4bd4ef9c487a7bcb036229ae5400c9e79ff6318d0722c94203b17fd43f50dc\"}"));

          UrlEncodedFormEntity formEntity = new UrlEncodedFormEntity(nameValuePairList, Encoding.UTF8);
          postMethod.Entity = formEntity;

          HttpResponse response = client.Execute(postMethod);
          string responseStr = EntityUtils.ToString(response.Entity);
          Console.WriteLine(responseStr);
        }
    }
}
