using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using DashFire.DataStore;
using DashFire;

namespace Lobby
{
    internal sealed class ActivationCodeSystem
    {
        //初始化激活码数据
        internal void InitActivationCodeData(List<DS_ActivationCode> activationCodeList)
        {
            if (activationCodeList.Count > 0)
            {
                foreach (var dataCode in activationCodeList)
                {
                    m_ActivationCodes.AddOrUpdate(dataCode.ActivationCode, dataCode.IsActivated, (s, b) => dataCode.IsActivated);
                }
            }
            m_IsDataLoaded = true;
        }
        //验证激活码   
        internal ActivateAccountResult CheckActivationCode(string code)
        {
            lock (m_Lock)
            {
                ActivateAccountResult ret = ActivateAccountResult.Error;
                bool isActivated = false;
                if (m_ActivationCodes.TryGetValue(code, out isActivated))
                {
                    if (isActivated == false)
                    {
                        m_ActivationCodes.AddOrUpdate(code, true, (s, b) => true);
                        ret = ActivateAccountResult.Success;
                    }
                    else
                    {
                        ret = ActivateAccountResult.InvalidCode;    //激活码已经使用
                    }
                }
                else
                {
                    ret = ActivateAccountResult.MistakenCode;     //激活码错误
                }
                return ret;
            }
        }
        private ConcurrentDictionary<string, bool> m_ActivationCodes = new ConcurrentDictionary<string, bool>();
        private object m_Lock = new object();
        private bool m_IsDataLoaded = false;
    }
}
