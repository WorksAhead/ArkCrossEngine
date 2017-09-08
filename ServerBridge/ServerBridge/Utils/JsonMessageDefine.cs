using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServerBridge
{
    public enum BillingRepState : int
    {
        Success = 200,        //网络请求处理成功
        NoPermission = 403,   //无访问权限（IP或签名不正确）
        EmptyQuery = 404,     //查询数据为空
        ServerFailure = 500,  //服务器失败
    }
    public class JsonVerifyAccountResultStatus
    {
        public string userstatus;
        public string status;
        public string userid;     //玩家账号在畅游平台的ID，在DFM中看作AccountId
    }
    public class JsonVerifyAccountResult
    {
        public int tag = 0;
        public int opcode = 0;
        public int state = 0;
        public JsonVerifyAccountResultStatus data = null;
        public int channelId = 0;
        public string error = "";
    }
}
