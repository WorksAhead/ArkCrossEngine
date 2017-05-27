using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal interface IModuleMailHandler
    {
        bool HaveMail(ModuleMailInfo moduleMailInfo, UserInfo user);
        MailInfo GetMail(ModuleMailInfo moduleMailInfo, UserInfo user, bool onlyAttachment);
    }
    /// <summary>
    /// 本邮件系统支持的邮件都是由系统发送，分为2类邮件：全员邮件与个人邮件。（全员邮件又分为普通全员邮件与模块相关全员邮件，普通全员邮件由邮件系统处理，
    /// 模块相关全员邮件的处理需要模块提供支持[接口IModuleMailHandler]。）
    /// 邮件都保存在邮件系统时，不保存到人身上。
    /// 全员邮件邮件系统为所有人保存一份，每个人记录已经收取过的全员邮件（邮件打开后点击收取按钮为收取，查看邮件不会收取，通常只有带附件的邮件可以收取）。
    /// </summary>
    /// <remarks>
    /// 除GlobalDataProcess处，不要直接调用本类的方法，邮件系统只在GlobalDataProcess线程进行处理，其它线程应调用GlobalDataProcess.QueueAction处理邮件。
    /// </remarks>
    internal sealed class MailSystem
    {
        //全部邮件列表，包含全局邮件、模块邮件和个人邮件
        internal List<MailInfo> TotalMailList
        {
            get
            {
                List<MailInfo> totalMailList = new List<MailInfo>(m_WholeMails);
                foreach (var mail in m_ModuleMails)
                {
                    totalMailList.Add(mail.NewDerivedMailInfo());
                }
                foreach (var userMailList in m_UserMails.Values)
                {
                    totalMailList.AddRange(userMailList);
                }
                return totalMailList;
            }
        }
        internal void InitMailData(List<MailInfo> mailList)
        {
            foreach (var mail in mailList)
            {
                if (mail.m_Module == ModuleMailTypeEnum.GowModule || mail.m_Module == ModuleMailTypeEnum.ArenaModule)
                {
                    ModuleMailInfo moduleMail = new ModuleMailInfo();
                    moduleMail.m_MailGuid = mail.m_MailGuid;
                    moduleMail.m_Module = mail.m_Module;
                    moduleMail.m_SendTime = mail.m_SendTime;
                    moduleMail.m_ExpiryDate = mail.m_ExpiryDate;
                    m_ModuleMails.Add(moduleMail);
                }
                else
                {
                    if (mail.m_Receiver == 0)
                    {
                        m_WholeMails.Add(mail);
                    }
                    else
                    {
                        List<MailInfo> userMailList = null;
                        if (!m_UserMails.TryGetValue(mail.m_Receiver, out userMailList))
                        {
                            userMailList = new List<MailInfo>();
                            m_UserMails.Add(mail.m_Receiver, userMailList);
                        }
                        userMailList.Add(mail);
                    }
                }
            }
            m_IsDataLoaded = true;
        }
        internal bool HaveMail(ulong userGuid)
        {
            bool ret = false;
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = dataProcess.GetUserInfo(userGuid);
            if (null != user)
            {
                List<MailInfo> userMails;
                if (m_UserMails.TryGetValue(userGuid, out userMails) && userMails.Count > 0)
                    ret = true;
                MailStateInfo mailStateInfo = user.MailStateInfo;
                int wholeMailCt = m_WholeMails.Count;
                for (int ix = 0; ix < wholeMailCt; ++ix)
                {
                    MailInfo mailInfo = m_WholeMails[ix];
                    if (mailInfo.m_LevelDemand <= user.Level && mailInfo.m_SendTime >= user.CreateTime && mailInfo.m_ExpiryDate >= DateTime.Now && !mailStateInfo.IsAlreadyReceived(mailInfo.m_MailGuid))
                    {
                        ret = true;
                        break;
                    }
                }
                int moduleMailCt = m_ModuleMails.Count;
                for (int ix = 0; ix < moduleMailCt; ++ix)
                {
                    ModuleMailInfo mailInfo = m_ModuleMails[ix];
                    IModuleMailHandler handler = GetModuleMailHandler(mailInfo.m_Module);
                    if (null != handler)
                    {
                        if (handler.HaveMail(mailInfo, user))
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }
            return ret;
        }
        internal void GetMailList(ulong userGuid)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = dataProcess.GetUserInfo(userGuid);
            if (null != user)
            {
                List<MailInfoForMessage> mailList = new List<MailInfoForMessage>();
                List<MailInfo> mails;
                if (m_UserMails.TryGetValue(userGuid, out mails))
                {
                    int ct = mails.Count;
                    for (int ix = 0; ix < ct; ++ix)
                    {
                        MailInfo mailInfo = mails[ix];
                        if (mailInfo.m_ExpiryDate >= DateTime.Now)
                        {
                            MailInfoForMessage mailInfoForMsg = new MailInfoForMessage();
                            mailInfoForMsg.m_AlreadyRead = mailInfo.m_AlreadyRead;
                            mailInfoForMsg.m_MailGuid = mailInfo.m_MailGuid;
                            mailInfoForMsg.m_Module = (int)mailInfo.m_Module;
                            mailInfoForMsg.m_Title = mailInfo.m_Title;
                            mailInfoForMsg.m_Sender = mailInfo.m_Sender;
                            mailInfoForMsg.m_SendTime = mailInfo.m_SendTime;
                            mailInfoForMsg.m_Text = mailInfo.m_Text;
                            mailInfoForMsg.m_Money = mailInfo.m_Money;
                            mailInfoForMsg.m_Gold = mailInfo.m_Gold;
                            mailInfoForMsg.m_Stamina = mailInfo.m_Stamina;
                            int itemCt = mailInfo.m_Items.Count;
                            if (itemCt > 0)
                            {
                                mailInfoForMsg.m_Items = new MailItemForMessage[itemCt];
                                for (int index = 0; index < itemCt; ++index)
                                {
                                    MailItemForMessage mailItem = new MailItemForMessage();
                                    mailItem.m_ItemId = mailInfo.m_Items[index].m_ItemId;
                                    mailItem.m_ItemNum = mailInfo.m_Items[index].m_ItemNum;
                                    mailInfoForMsg.m_Items[index] = mailItem;
                                }
                            }
                            mailList.Add(mailInfoForMsg);
                        }
                    }
                }
                MailStateInfo mailStateInfo = user.MailStateInfo;
                //这里不对用户数据加锁，因为用户的邮件状态的改变都在这个线程完成（除上线时的数据加载）
                int wholeMailCt = m_WholeMails.Count;
                for (int ix = 0; ix < wholeMailCt; ++ix)
                {
                    MailInfo mailInfo = m_WholeMails[ix];
                    if (mailInfo.m_LevelDemand <= user.Level && mailInfo.m_SendTime >= user.CreateTime && mailInfo.m_ExpiryDate >= DateTime.Now && !mailStateInfo.IsAlreadyReceived(mailInfo.m_MailGuid))
                    {
                        if (!mailStateInfo.HaveMail(mailInfo.m_MailGuid))
                        {
                            mailStateInfo.AddMail(mailInfo.m_MailGuid, mailInfo.m_ExpiryDate);
                        }
                        MailInfoForMessage mailInfoForMsg = new MailInfoForMessage();
                        mailInfoForMsg.m_AlreadyRead = mailStateInfo.IsAlreadyRead(mailInfo.m_MailGuid);
                        mailInfoForMsg.m_MailGuid = mailInfo.m_MailGuid;
                        mailInfoForMsg.m_Module = (int)mailInfo.m_Module;
                        mailInfoForMsg.m_Title = mailInfo.m_Title;
                        mailInfoForMsg.m_Sender = mailInfo.m_Sender;
                        mailInfoForMsg.m_SendTime = mailInfo.m_SendTime;
                        mailInfoForMsg.m_Text = mailInfo.m_Text;
                        mailInfoForMsg.m_Money = mailInfo.m_Money;
                        mailInfoForMsg.m_Gold = mailInfo.m_Gold;
                        mailInfoForMsg.m_Stamina = mailInfo.m_Stamina;
                        int itemCt = mailInfo.m_Items.Count;
                        if (itemCt > 0)
                        {
                            mailInfoForMsg.m_Items = new MailItemForMessage[itemCt];
                            for (int index = 0; index < itemCt; ++index)
                            {
                                MailItemForMessage mailItem = new MailItemForMessage();
                                mailItem.m_ItemId = mailInfo.m_Items[index].m_ItemId;
                                mailItem.m_ItemNum = mailInfo.m_Items[index].m_ItemNum;
                                mailInfoForMsg.m_Items[index] = mailItem;
                            }
                        }
                        mailList.Add(mailInfoForMsg);
                    }
                }
                int moduleMailCt = m_ModuleMails.Count;
                for (int ix = 0; ix < moduleMailCt; ++ix)
                {
                    ModuleMailInfo moduleMailInfo = m_ModuleMails[ix];
                    if (moduleMailInfo.m_SendTime >= user.CreateTime && moduleMailInfo.m_ExpiryDate >= DateTime.Now && !mailStateInfo.IsAlreadyReceived(moduleMailInfo.m_MailGuid))
                    {
                        if (!mailStateInfo.HaveMail(moduleMailInfo.m_MailGuid))
                        {
                            mailStateInfo.AddMail(moduleMailInfo.m_MailGuid, moduleMailInfo.m_ExpiryDate);
                        }
                        IModuleMailHandler handler = GetModuleMailHandler(moduleMailInfo.m_Module);
                        if (null != handler)
                        {
                            MailInfo mailInfo = handler.GetMail(moduleMailInfo, user, false);
                            if (null != mailInfo)
                            {
                                MailInfoForMessage mailInfoForMsg = new MailInfoForMessage();
                                mailInfoForMsg.m_AlreadyRead = mailStateInfo.IsAlreadyRead(moduleMailInfo.m_MailGuid);
                                mailInfoForMsg.m_MailGuid = mailInfo.m_MailGuid;
                                mailInfoForMsg.m_Module = (int)mailInfo.m_Module;
                                mailInfoForMsg.m_Title = mailInfo.m_Title;
                                mailInfoForMsg.m_Sender = mailInfo.m_Sender;
                                mailInfoForMsg.m_SendTime = mailInfo.m_SendTime;
                                mailInfoForMsg.m_Text = mailInfo.m_Text;
                                mailInfoForMsg.m_Money = mailInfo.m_Money;
                                mailInfoForMsg.m_Gold = mailInfo.m_Gold;
                                mailInfoForMsg.m_Stamina = mailInfo.m_Stamina;
                                int itemCt = mailInfo.m_Items.Count;
                                if (itemCt > 0)
                                {
                                    mailInfoForMsg.m_Items = new MailItemForMessage[itemCt];
                                    for (int index = 0; index < itemCt; ++index)
                                    {
                                        MailItemForMessage mailItem = new MailItemForMessage();
                                        mailItem.m_ItemId = mailInfo.m_Items[index].m_ItemId;
                                        mailItem.m_ItemNum = mailInfo.m_Items[index].m_ItemNum;
                                        mailInfoForMsg.m_Items[index] = mailItem;
                                    }
                                }
                                mailList.Add(mailInfoForMsg);
                            }
                        }
                    }
                }
                JsonMessageSyncMailList syncMailListMsg = new JsonMessageSyncMailList();
                syncMailListMsg.m_Guid = userGuid;
                syncMailListMsg.m_Mails = mailList.ToArray();
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, syncMailListMsg);
            }
        }
        internal void SendUserMail(MailInfo userMail, int validityPeriod)
        {
            userMail.m_MailGuid = GenMailGuid();
            userMail.m_SendTime = DateTime.Now;
            userMail.m_ExpiryDate = userMail.m_SendTime.AddDays(validityPeriod);
            List<MailInfo> mails = null;
            if (!m_UserMails.TryGetValue(userMail.m_Receiver, out mails))
            {
                mails = new List<MailInfo>();
                m_UserMails.Add(userMail.m_Receiver, mails);
            }
            mails.Add(userMail);
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = dataProcess.GetUserInfo(userMail.m_Receiver);
            if (null != user && user.CurrentState != UserState.DropOrOffline)
            {
                JsonMessageNotifyNewMail newMailMsg = new JsonMessageNotifyNewMail();
                newMailMsg.m_Guid = userMail.m_Receiver;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, newMailMsg);
            }
        }
        internal void SendWholeMail(MailInfo wholeMail, int validityPeriod)
        {
            wholeMail.m_MailGuid = GenMailGuid();
            wholeMail.m_SendTime = DateTime.Now;
            wholeMail.m_ExpiryDate = wholeMail.m_SendTime.AddDays(validityPeriod);
            m_WholeMails.Add(wholeMail);
            JsonMessageNotifyNewMail newMailMsg = new JsonMessageNotifyNewMail();
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            dataProcess.VisitUsers((UserInfo userInfo) =>
            {
                newMailMsg.m_Guid = userInfo.Guid;
                JsonMessageDispatcher.SendDcoreMessage(userInfo.NodeName, newMailMsg);
            });
        }
        internal void SendModuleMail(ModuleMailInfo moduleMail, int validityPeriod)
        {
            moduleMail.m_MailGuid = GenMailGuid();
            moduleMail.m_SendTime = DateTime.Now;
            moduleMail.m_ExpiryDate = moduleMail.m_SendTime.AddDays(validityPeriod);
            m_ModuleMails.Add(moduleMail);
            JsonMessageNotifyNewMail newMailMsg = new JsonMessageNotifyNewMail();
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            dataProcess.VisitUsers((UserInfo userInfo) =>
            {
                newMailMsg.m_Guid = userInfo.Guid;
                JsonMessageDispatcher.SendDcoreMessage(userInfo.NodeName, newMailMsg);
            });
        }
        internal void ReadMail(ulong userGuid, ulong mailGuid)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = dataProcess.GetUserInfo(userGuid);
            if (null != user)
            {
                List<MailInfo> mails;
                if (m_UserMails.TryGetValue(userGuid, out mails))
                {
                    if (null != mails)
                    {
                        int ct = mails.Count;
                        int index = 0;
                        for (; index < ct; ++index)
                        {
                            if (mails[index].m_MailGuid == mailGuid)
                            {
                                MailInfo info = mails[index];
                                info.m_AlreadyRead = true;
                                break;
                            }
                        }
                    }
                }
                MailStateInfo mailStateInfo = user.MailStateInfo;
                int wholeCt = m_WholeMails.Count;
                for (int index = 0; index < wholeCt; ++index)
                {
                    MailInfo info = m_WholeMails[index];
                    if (info.m_MailGuid == mailGuid)
                    {
                        mailStateInfo.ReadMail(mailGuid);
                        break;
                    }
                }
                int moduleCt = m_ModuleMails.Count;
                for (int index = 0; index < moduleCt; ++index)
                {
                    ModuleMailInfo mailInfo = m_ModuleMails[index];
                    if (mailInfo.m_MailGuid == mailGuid)
                    {
                        mailStateInfo.ReadMail(mailGuid);
                        break;
                    }
                }
            }
        }
        internal void ReceiveMail(ulong userGuid, ulong mailGuid)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = dataProcess.GetUserInfo(userGuid);
            if (null != user)
            {
                List<MailInfo> mails;
                if (m_UserMails.TryGetValue(userGuid, out mails))
                {
                    if (null != mails)
                    {
                        MailInfo info = null;
                        int ct = mails.Count;
                        int index = 0;
                        for (; index < ct; ++index)
                        {
                            if (mails[index].m_MailGuid == mailGuid)
                            {
                                info = mails[index];
                                break;
                            }
                        }
                        if (null != info)
                        {
                            mails.RemoveAt(index);
                            ExtractMailAttachment(info, userGuid);
                        }
                    }
                }
                MailStateInfo mailStateInfo = user.MailStateInfo;
                if (!mailStateInfo.IsAlreadyReceived(mailGuid))
                {
                    int wholeCt = m_WholeMails.Count;
                    for (int index = 0; index < wholeCt; ++index)
                    {
                        MailInfo info = m_WholeMails[index];
                        if (info.m_MailGuid == mailGuid)
                        {
                            mailStateInfo.ReceiveMail(mailGuid);
                            if (info.m_LevelDemand <= user.Level && info.m_SendTime >= user.CreateTime && info.m_ExpiryDate >= DateTime.Now)
                            {
                                ExtractMailAttachment(info, userGuid);
                            }
                        }
                    }
                    int moduleCt = m_ModuleMails.Count;
                    for (int index = 0; index < moduleCt; ++index)
                    {
                        ModuleMailInfo mailInfo = m_ModuleMails[index];
                        if (mailInfo.m_MailGuid == mailGuid)
                        {
                            mailStateInfo.ReceiveMail(mailGuid);
                            if (mailInfo.m_SendTime >= user.CreateTime && mailInfo.m_ExpiryDate >= DateTime.Now)
                            {
                                IModuleMailHandler handler = GetModuleMailHandler(mailInfo.m_Module);
                                if (null != handler)
                                {
                                    MailInfo info = handler.GetMail(mailInfo, user, true);
                                    if (null != info)
                                    {
                                        ExtractMailAttachment(info, userGuid);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
        internal void Tick()
        {
            var ds_thread = LobbyServer.Instance.DataStoreThread;
            if (ds_thread.DataStoreAvailable)
            {
                if (!m_IsDataLoaded)
                {
                    return;
                }
            }
            long curTime = ArkCrossEngine.TimeUtility.GetServerMilliseconds();
            if (m_LastTickTime + c_TickInterval < curTime)
            {
                m_LastTickTime = curTime;
                //清理过期邮件
                int ct = m_WholeMails.Count;
                for (int index = ct - 1; index >= 0; --index)
                {
                    MailInfo mailInfo = m_WholeMails[index];
                    if (null != mailInfo)
                    {
                        if (mailInfo.m_ExpiryDate < DateTime.Now)
                        {
                            m_WholeMails.RemoveAt(index);
                        }
                    }
                }
                int count = m_ModuleMails.Count;
                for (int index = count - 1; index >= 0; --index)
                {
                    ModuleMailInfo moduleMail = m_ModuleMails[index];
                    if (null != moduleMail)
                    {
                        if (moduleMail.m_ExpiryDate < DateTime.Now)
                        {
                            m_ModuleMails.RemoveAt(index);
                        }
                    }
                }
                foreach (List<MailInfo> mails in m_UserMails.Values)
                {
                    int mailCt = mails.Count;
                    for (int index = mailCt - 1; index >= 0; --index)
                    {
                        MailInfo mailInfo = mails[index];
                        if (null != mailInfo)
                        {
                            if (mailInfo.m_ExpiryDate < DateTime.Now)
                            {
                                mails.RemoveAt(index);
                            }
                        }
                    }
                }
            }
        }
        internal void RegisterModuleMailHandler(ModuleMailTypeEnum module, IModuleMailHandler handler)
        {
            if (m_ModuleMailHandlers.ContainsKey(module))
            {
                m_ModuleMailHandlers[module] = handler;
            }
            else
            {
                m_ModuleMailHandlers.Add(module, handler);
            }
        }
        internal List<ModuleMailInfo> GetModuleMailList(ModuleMailTypeEnum moduleType)
        {
            List<ModuleMailInfo> moduleMails = new List<ModuleMailInfo>();
            foreach (ModuleMailInfo mail in m_ModuleMails)
            {
                if (mail.m_Module == moduleType)
                {
                    moduleMails.Add(mail);
                }
            }
            return moduleMails;
        }

        private void ExtractMailAttachment(MailInfo info, ulong userGuid)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            dataProcess.DispatchAction(dataProcess.DoAddAssets, userGuid, info.m_Money, info.m_Gold, 0, info.m_Stamina, GainConsumePos.Mail.ToString());

            int itemCt = info.m_Items.Count;
            for (int itemIx = 0; itemIx < itemCt; ++itemIx)
            {
                MailItem item = info.m_Items[itemIx];
                dataProcess.DispatchAction(dataProcess.DoAddItem, userGuid, item.m_ItemId, item.m_ItemNum, GainConsumePos.Mail.ToString());
            }
        }
        private IModuleMailHandler GetModuleMailHandler(ModuleMailTypeEnum module)
        {
            IModuleMailHandler handler;
            m_ModuleMailHandlers.TryGetValue(module, out handler);
            return handler;
        }
        private ulong GenMailGuid()
        {
            return LobbyServer.Instance.GlobalDataProcessThread.GenerateMailGuid();
        }

        private Dictionary<ulong, List<MailInfo>> m_UserMails = new Dictionary<ulong, List<MailInfo>>();
        private List<MailInfo> m_WholeMails = new List<MailInfo>();
        private List<ModuleMailInfo> m_ModuleMails = new List<ModuleMailInfo>();

        private Dictionary<ModuleMailTypeEnum, IModuleMailHandler> m_ModuleMailHandlers = new Dictionary<ModuleMailTypeEnum, IModuleMailHandler>();

        private long m_LastTickTime = 0;
        private const long c_TickInterval = 60000;

        private long m_LastSaveTime = 0;
        private const int c_SaveTimeInterval = 180000;    //邮件数据的存储间隔

        private bool m_IsDataLoaded = false;
    }
}
