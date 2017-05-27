using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;
using DashFire.Billing;
using CSharpCenterClient;
using Messenger;
using Google.ProtocolBuffers;
using Newtonsoft.Json;
using ServerBridge;
using DashFire;
using CodeScales.Http;
using CodeScales.Http.Methods;
using CodeScales.Http.Entity;
using CodeScales.Http.Common;

internal class BillingSystem
{
  private void Init(string[] args)
  {
    m_NameHandleCallback = this.OnNameHandleChanged;
    m_MsgCallback = this.OnMessage;
    m_CmdCallback = this.OnCommand;
    CenterClientApi.Init("bridge", args.Length, args, m_NameHandleCallback, m_MsgCallback, m_CmdCallback);

    m_Channel = new PBChannel(DashFire.Billing.MessageMapping.Query,
                  DashFire.Billing.MessageMapping.Query);
    m_Channel.DefaultServiceName = "Lobby";
    m_Channel.Register<LB_VerifyAccount>(HandleVerifyAccount);

    LogSys.Init("./config/logconfig.xml");

    StringBuilder sb = new StringBuilder(256);
    if (CenterClientApi.GetConfig("AppKey", sb, 256)) {
      m_AppKey = sb.ToString();
    }
    if (CenterClientApi.GetConfig("AppSecret", sb, 256)) {
      m_AppSecret = sb.ToString();
    }
    if (CenterClientApi.GetConfig("BillingServerUrl", sb, 256)) {
      m_BillingServerUrl = sb.ToString();
    }
    if (CenterClientApi.GetConfig("TestBillingServerUrl", sb, 256)) {
      m_TestBillingServerUrl = sb.ToString();
    }
    if (CenterClientApi.GetConfig("HttpRequestTimeout", sb, 256)) {
      m_HttpRequestTimeout = int.Parse(sb.ToString());
    }

    LogSys.Log(LOG_TYPE.INFO, "BillingSystem initialized");
  }
  private void Loop()
  {
    try {
      while (CenterClientApi.IsRun()) {
        CenterClientApi.Tick();
        Thread.Sleep(10);
      }
    }
    catch (Exception ex) {
      LogSys.Log(LOG_TYPE.ERROR, "ServerBridge.Loop throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
    }
  }
  private void Release()
  {
    m_TaskDispatcher.StopTaskThreads();
    CenterClientApi.Release();
    Thread.Sleep(3000);
    //简单点，直接kill掉自己
    System.Diagnostics.Process.GetCurrentProcess().Kill();
  }
  private void OnNameHandleChanged(bool addOrUpdate, string name, int handle)
  {
    try {
      m_Channel.OnUpdateNameHandle(addOrUpdate, name, handle);
    } catch (Exception ex) {
      LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
    }
  }
  private void OnCommand(int src, int dest, string command)
  {
    try {
      if (0 == command.CompareTo("QuitServerBridge")) {
        LogSys.Log(LOG_TYPE.MONITOR, "receive {0} command, quit", command);
        CenterClientApi.Quit();
      }
    } catch (Exception ex) {
      LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
    }
  }
  private void OnMessage(uint seq, int source_handle, int dest_handle,
      IntPtr data, int len)
  {
    try {
      byte[] bytes = new byte[len];
      Marshal.Copy(data, bytes, 0, len);
      m_Channel.Dispatch(source_handle, seq, bytes);
    } catch (Exception ex) {
      LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
    }
  }
  private void HandleVerifyAccount(LB_VerifyAccount msg, PBChannel channel, int handle, uint seq)
  {
    LogSys.Log(LOG_TYPE.INFO, "request billing to verify account: {0} ", msg.Account);
    //m_TaskDispatcher.DispatchAction(VerifyAccountWorker, msg, channel);
    m_TaskDispatcher.DispatchAction(VerifyAccountWorkerWithHttpRequest, msg, channel);
  }

  private void VerifyAccountWorker(LB_VerifyAccount msg, PBChannel channel)
  {
    try {
      int tag = CreateTag();
      string signParam = string.Format("{0}{1}{2}{3}{4}{5}", msg.OpCode, msg.Data, m_AppKey, m_AppSecret, tag, msg.ChannelId);
      string sign = CreateSign(signParam);

      HttpClient client = new HttpClient();
      client.Timeout = m_HttpRequestTimeout;
      HttpPost postMethod = new HttpPost(new Uri(m_TestBillingServerUrl));
      
      postMethod.Headers.Add("appkey", m_AppKey);
      postMethod.Headers.Add("sign", sign);
      postMethod.Headers.Add("tag", tag.ToString());
      postMethod.Headers.Add("opcode", msg.OpCode.ToString());
      postMethod.Headers.Add("channelId", msg.ChannelId.ToString());

      List<NameValuePair> nameValuePairList = new List<NameValuePair>();
      nameValuePairList.Add(new NameValuePair("data", msg.Data));

      UrlEncodedFormEntity formEntity = new UrlEncodedFormEntity(nameValuePairList, Encoding.UTF8);
      postMethod.Entity = formEntity;

      LogSys.Log(LOG_TYPE.INFO, "Account:{0}, HttpPost headers. appkey:{1}, sign:{2}, tag:{3}, opcode:{4}, channelId:{5}",
        msg.Account, m_AppKey, sign, tag, msg.OpCode, msg.ChannelId);
      LogSys.Log(LOG_TYPE.INFO, "Account:{0}, HttpPost parameters. data:{1}", msg.Account, msg.Data);
      //============================================     

      HttpResponse response = client.Execute(postMethod);
      string responseStr = EntityUtils.ToString(response.Entity);

      DestroyTag(tag);
      
      //==================================================
      LogSys.Log(LOG_TYPE.INFO, "Account:{0}, Response:{1}", msg.Account, responseStr);
      //==================================================
      
      JsonVerifyAccountResult result = JsonConvert.DeserializeObject(responseStr, typeof(JsonVerifyAccountResult)) as JsonVerifyAccountResult;
      var reply = BL_VerifyAccountResult.CreateBuilder();
      reply.Account = msg.Account;
      reply.OpCode = result.opcode;
      reply.ChannelId = result.channelId;
      reply.AccountId = "";
      reply.Result = false;
      int repState = result.state;
      if (repState == (int)BillingRepState.Success && result.data != null) {
        int status = int.Parse(result.data.status);
        if (status == 1 && result.channelId == msg.ChannelId && result.opcode == msg.OpCode) {
          reply.AccountId = result.data.userid;
          reply.Result = true;
        }
      }
      if (reply.Result == true) {
        LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Account verify success. Account:{0} ID:{1}", reply.Account, reply.AccountId);
      } else {
        LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Yellow, "Account verify failed. Account:{0} Msg:{1}", reply.Account, result.error);
      }
      channel.Send(reply.Build());
    } catch (Exception ex) {
      LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "Exception Type:{0}", ex.GetType().ToString());
      LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "Exception:{0}\n{1}", ex.Message, ex.StackTrace);
    }
  }

  private void VerifyAccountWorkerWithHttpRequest(LB_VerifyAccount msg, PBChannel channel)
  {
    HttpWebRequest myHttpWebRequest = null;
    HttpWebResponse myHttpWebResponse = null;
    try {
      int tag = CreateTag();
      string signParam = string.Format("{0}{1}{2}{3}{4}{5}", msg.OpCode, msg.Data, m_AppKey, m_AppSecret, tag, msg.ChannelId);
      string sign = CreateSign(signParam);
      IDictionary<string, string> headers = new Dictionary<string, string>();
      headers.Add("appkey", m_AppKey);
      headers.Add("sign", sign);
      headers.Add("tag", tag.ToString());
      headers.Add("opcode", msg.OpCode.ToString());
      headers.Add("channelId", msg.ChannelId.ToString());
      IDictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("data", msg.Data);
      LogSys.Log(LOG_TYPE.INFO, "Account:{0}, HttpPost headers. appkey:{1}, sign:{2}, tag:{3}, opcode:{4}, channelId:{5}",
        msg.Account, m_AppKey, sign, tag, msg.OpCode, msg.ChannelId);
      LogSys.Log(LOG_TYPE.INFO, "Account:{0}, HttpPost parameters. data:{1}", msg.Account,msg.Data);
      //============================================     
      myHttpWebRequest = HttpWebUtility.CreatePostHttpRequest(m_TestBillingServerUrl, headers, parameters, m_HttpRequestTimeout);
      myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
      string responseStr = string.Empty;
      using (Stream streamResponse = myHttpWebResponse.GetResponseStream()) {
        using (StreamReader readStream = new StreamReader(streamResponse, Encoding.UTF8)) {
          responseStr = readStream.ReadToEnd();
          readStream.Close();
        }
        streamResponse.Flush();
        streamResponse.Close();
      }
      DestroyTag(tag);
      //==================================================
      LogSys.Log(LOG_TYPE.INFO, "Account:{0}, Response:{1}", msg.Account, responseStr);
      //==================================================
      JsonVerifyAccountResult result = JsonConvert.DeserializeObject(responseStr, typeof(JsonVerifyAccountResult)) as JsonVerifyAccountResult;
      var reply = BL_VerifyAccountResult.CreateBuilder();
      reply.Account = msg.Account;
      reply.OpCode = result.opcode;
      reply.ChannelId = result.channelId;
      reply.AccountId = "";
      reply.Result = false;
      int repState = result.state;
      if (repState == (int)BillingRepState.Success && result.data != null) {
        int status = int.Parse(result.data.status);
        if (status == 1 && result.channelId == msg.ChannelId && result.opcode == msg.OpCode) {
          reply.AccountId = result.data.userid;
          reply.Result = true;
        }
      }
      if (reply.Result == true) {
        LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Account verify success. Account:{0} ID:{1}", reply.Account, reply.AccountId);
      } else {
        LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Yellow, "Account verify failed. Account:{0} Msg:{1}", reply.Account, result.error);
      }
      channel.Send(reply.Build());
    }
    catch (Exception ex) {
      LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "Exception Type:{0}", ex.GetType().ToString());
      LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "Exception:{0}\n{1}", ex.Message, ex.StackTrace);
    }
    finally {
      if (myHttpWebResponse != null) {
        myHttpWebResponse.Close();
      }
      if (myHttpWebRequest != null) {
        myHttpWebRequest.Abort();
      }
    }
  }

  /// <summary>
  /// 计算数字签名
  /// </summary>
  /// <param name="input">opcode+data+appkey+appsecret+tag+channel</param>
  /// <returns></returns>
  private string CreateSign(string param)
  {
    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
    byte[] bs = Encoding.UTF8.GetBytes(param);
    bs = md5.ComputeHash(bs);
    StringBuilder s = new StringBuilder();
    foreach (byte b in bs) {
      s.Append(b.ToString("x2"));
    }
    return s.ToString().Substring(8, 16);
  }
  /// <summary>
  /// 生成事务唯一标识
  /// </summary>
  /// <returns></returns>
  private int CreateTag()
  {
    int r = 0;
    while (true) {
      r = m_Random.Next(1, 9999999);
      if (m_Tags.ContainsKey(r) == false) {
        m_Tags.TryAdd(r, 0);
        break;
      }
    }
    return r;
  }
  /// <summary>
  /// 销毁事务唯一标识
  /// </summary>
  /// <param name="tag">待回收的标识</param>
  /// <returns></returns>
  private bool DestroyTag(int tag)
  {
    int v = 0;
    return m_Tags.TryRemove(tag, out v);
  }

  private PBChannel m_Channel = null;
  private ArkCrossEngine.MyServerTaskDispatcher m_TaskDispatcher = new ArkCrossEngine.MyServerTaskDispatcher(64);

  private ConcurrentDictionary<int, int> m_Tags = new ConcurrentDictionary<int, int>();
  private Random m_Random = new Random();

  private string m_AppKey = "1394628137031";
  private string m_AppSecret = "5ae88850e3af41fab5c3e4419fbb15be";
  private string m_BillingServerUrl = "http://mobilebilling.changyou.com/billing";
  private string m_TestBillingServerUrl = "http://tmobilebilling.changyou.com/billing";
  private int m_HttpRequestTimeout = 10000;

  private CenterClientApi.HandleNameHandleChangedCallback m_NameHandleCallback = null;
  private CenterClientApi.HandleMessageCallback m_MsgCallback = null;
  private CenterClientApi.HandleCommandCallback m_CmdCallback = null;
  
  internal static void Main(string[] args)
  {
    BillingSystem sys = new BillingSystem();
    sys.Init(args);
    sys.Loop();
    sys.Release();
  }
}