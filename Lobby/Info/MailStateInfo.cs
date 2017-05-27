using System;
using System.Collections.Generic;

namespace Lobby
{
  internal sealed class MailState
  {
    internal ulong m_MailGuid;
    internal DateTime m_ExpiryDate;
    internal bool m_AlreadyRead;
    internal bool m_AlreadyReceived;
  }
  internal sealed class MailStateInfo
  {
    internal object Lock
    {
      get { return m_Lock; }
    }
    //外部访问时先对Lock锁定！！！
    internal Dictionary<ulong, MailState> WholeMailStates
    {
      get { return m_WholeMailStates; }
    }
    internal int WholeMailCount
    {
      get
      {
        return m_WholeMailStates.Count;
      }
    }
    internal bool HaveMail(ulong mailGuid)
    {
      bool ret = false;
      lock (m_Lock) {
        if (m_WholeMailStates.ContainsKey(mailGuid)) {
          ret = true;
        }
      }
      return ret;
    }
    internal void AddMail(ulong mailGuid, DateTime expiryDate)
    {
      lock (m_Lock) {
        MailState state = null;
        if (!m_WholeMailStates.ContainsKey(mailGuid)) {
          state = new MailState();
          state.m_MailGuid = mailGuid;
          state.m_ExpiryDate = expiryDate;
          state.m_AlreadyRead = false;
          state.m_AlreadyReceived = false;
          m_WholeMailStates.Add(mailGuid, state);
        }
      }
    }
    internal void RemoveExpiredMails()
    {
      lock (m_Lock) {
        DateTime nowDate = DateTime.Now;
        foreach (MailState state in m_WholeMailStates.Values) {
          if (state.m_ExpiryDate < nowDate) {
            m_ExpiredMails.Add(state.m_MailGuid);
          }
        }
        foreach (ulong guid in m_ExpiredMails) {
          m_WholeMailStates.Remove(guid);
        }
        m_ExpiredMails.Clear();
      }
    }
    internal bool IsAlreadyRead(ulong mailGuid)
    {
      bool ret = true;
      lock (m_Lock) {
        MailState state;
        if (m_WholeMailStates.TryGetValue(mailGuid, out state)) {
          ret = state.m_AlreadyRead;
        }
      }
      return ret;
    }
    internal void ReadMail(ulong mailGuid)
    {
      lock (m_Lock) {
        MailState state;
        if (m_WholeMailStates.TryGetValue(mailGuid, out state)) {
          state.m_AlreadyRead = true;
        }
      }
    }
    internal bool IsAlreadyReceived(ulong mailGuid)
    {
      bool ret = false;
      lock (m_Lock) {
        MailState state;
        if (m_WholeMailStates.TryGetValue(mailGuid, out state)) {
          ret = state.m_AlreadyReceived;
        }
      }
      return ret;
    }
    internal void ReceiveMail(ulong mailGuid)
    {
      lock (m_Lock) {
        MailState state;
        if (m_WholeMailStates.TryGetValue(mailGuid, out state)) {
          state.m_AlreadyReceived = true;
        }
      }
    }
    internal void Reset()
    {
      m_Lock = new object();
      m_WholeMailStates.Clear();
      m_ExpiredMails.Clear();
    }

    private object m_Lock = new object();
    private Dictionary<ulong, MailState> m_WholeMailStates = new Dictionary<ulong, MailState>();
    private List<ulong> m_ExpiredMails = new List<ulong>();
  }
}
