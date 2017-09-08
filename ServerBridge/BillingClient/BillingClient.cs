using System;
using System.Collections.Generic;
using Messenger;
using Google.ProtocolBuffers;
using DashFire;
using ArkCrossEngine;

namespace DashFire.Billing
{
    public sealed class BillingClient
    {
        public delegate void VerifyAccountCB(string account, bool ret, string accountId);

        public BillingClient(PBChannel channel, MyServerThread thread)
        {
            channel_ = channel;
            m_RunningThread = thread;

            channel_.Register<BL_VerifyAccountResult>(OnVerifyAccountResult);

            m_VerifyAccountTimeout = new Timeout<VerifyAccountCB>();
            m_VerifyAccountTimeout.DefaultTimeoutMS = 10000;      //超时时间
            m_VerifyAccountWatch = new OperationWatch();
        }

        public void VerifyAccount(string account, int opcode, int channelId, string data, VerifyAccountCB cb)
        {
            uint msgId = MessageMapping.Query(typeof(LB_VerifyAccount));
            string timeoutKey = string.Format("{0}:{1}", msgId, account);
            if (m_VerifyAccountWatch.Exists(timeoutKey))
            {
                cb(account, false, "");
            }
            else
            {
                LB_VerifyAccount.Builder builder = LB_VerifyAccount.CreateBuilder();
                builder.Account = account;
                builder.OpCode = opcode;
                builder.ChannelId = channelId;
                builder.Data = data;
                if (!channel_.Send(builder.Build()))
                {
                    cb(account, false, "");
                }
                else
                {
                    m_VerifyAccountWatch.Add(timeoutKey);
                    m_VerifyAccountTimeout.Set(timeoutKey, cb, () => cb.Invoke(account, false, ""));
                }
            }
        }

        public void Tick()
        {
            m_VerifyAccountWatch.Tick();
            m_VerifyAccountTimeout.Tick();
        }

        private void OnVerifyAccountResult(BL_VerifyAccountResult msg, PBChannel channel, int src, uint seq)
        {
            m_RunningThread.QueueAction(DoVerfifyAccountResult, msg, channel, src, seq);
        }
        private void DoVerfifyAccountResult(BL_VerifyAccountResult msg, PBChannel channel, int src, uint seq)
        {
            uint msgId = MessageMapping.Query(typeof(LB_VerifyAccount));
            string timeoutKey = string.Format("{0}:{1}", msgId, msg.Account);
            m_VerifyAccountWatch.Remove(timeoutKey);
            VerifyAccountCB cb = m_VerifyAccountTimeout.Get(timeoutKey);
            if (cb != null)
            {
                cb(msg.Account, msg.Result, msg.AccountId);
                m_VerifyAccountTimeout.Remove(timeoutKey);
            }
        }

        private PBChannel channel_;
        private MyServerThread m_RunningThread;
        private Timeout<VerifyAccountCB> m_VerifyAccountTimeout;
        private OperationWatch m_VerifyAccountWatch;
    }
}