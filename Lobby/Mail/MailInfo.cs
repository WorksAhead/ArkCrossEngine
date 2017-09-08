using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal sealed class MailItem
    {
        internal int m_ItemId;
        internal int m_ItemNum;
    }
    internal sealed class MailInfo
    {
        internal bool m_AlreadyRead = false;
        internal ulong m_MailGuid = 0;
        internal string m_Title = "";
        internal string m_Sender = "";
        internal ModuleMailTypeEnum m_Module = ModuleMailTypeEnum.None;
        internal DateTime m_SendTime = new DateTime();
        internal DateTime m_ExpiryDate = new DateTime();
        internal string m_Text = "";
        internal ulong m_Receiver = 0;
        internal int m_LevelDemand = 0;
        internal List<MailItem> m_Items = new List<MailItem>();
        internal int m_Money = 0;
        internal int m_Gold = 0;
        internal int m_Stamina = 0;
    }
    internal sealed class ModuleMailInfo
    {
        internal ulong m_MailGuid;
        internal ModuleMailTypeEnum m_Module;
        internal DateTime m_SendTime;
        internal DateTime m_ExpiryDate;

        internal MailInfo NewDerivedMailInfo()
        {
            MailInfo mailInfo = new MailInfo();
            mailInfo.m_MailGuid = m_MailGuid;
            mailInfo.m_Module = m_Module;
            mailInfo.m_SendTime = m_SendTime;
            mailInfo.m_ExpiryDate = m_ExpiryDate;
            return mailInfo;
        }
    }
}
