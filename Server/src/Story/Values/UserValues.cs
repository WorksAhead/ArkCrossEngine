using System;
using System.Collections.Generic;
using StorySystem;
using DashFire;
using ArkCrossEngine;

namespace DashFire.Story.Values
{
  internal sealed class UserIdListValue : IStoryValue<object>
  {
    public void InitFromDsl(ScriptableData.ISyntaxComponent param)
    {
      ScriptableData.CallData callData = param as ScriptableData.CallData;
      if (null != callData && callData.GetId() == "useridlist") {
      }
    }
    public IStoryValue<object> Clone()
    {
      UserIdListValue val = new UserIdListValue();
      val.m_HaveValue = m_HaveValue;
      val.m_Value = m_Value;
      return val;
    }
    public void Evaluate(object iterator, object[] args)
    {
      m_Iterator = iterator;
      m_Args = args;
    }
    public void Evaluate(StoryInstance instance)
    {
      TryUpdateValue(instance);
    }
    public void Analyze(StoryInstance instance)
    {
    }
    public bool HaveValue
    {
      get
      {
        return m_HaveValue;
      }
    }
    public object Value
    {
      get
      {
        return m_Value;
      }
    }

    private void TryUpdateValue(StoryInstance instance)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        List<object> users = new List<object>();
        scene.UserManager.Users.VisitValues((UserInfo userInfo) => {
          users.Add(userInfo.GetId());
        });
        m_HaveValue = true;
        m_Value = users;
      }
    }

    private object m_Iterator = null;
    private object[] m_Args = null;

    private bool m_HaveValue;
    private object m_Value;
  }
  internal sealed class WinUserIdValue : IStoryValue<object>
  {
    public void InitFromDsl(ScriptableData.ISyntaxComponent param)
    {
      ScriptableData.CallData callData = param as ScriptableData.CallData;
      if (null != callData && callData.GetId() == "winuserid") {
      }
    }
    public IStoryValue<object> Clone()
    {
      WinUserIdValue val = new WinUserIdValue();
      val.m_HaveValue = m_HaveValue;
      val.m_Value = m_Value;
      return val;
    }
    public void Evaluate(object iterator, object[] args)
    {
      m_Iterator = iterator;
      m_Args = args;
    }
    public void Evaluate(StoryInstance instance)
    {
      TryUpdateValue(instance);
    }
    public void Analyze(StoryInstance instance)
    {
    }
    public bool HaveValue
    {
      get
      {
        return m_HaveValue;
      }
    }
    public object Value
    {
      get
      {
        return m_Value;
      }
    }

    private void TryUpdateValue(StoryInstance instance)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {        
        m_HaveValue = true;
        if (scene.IsPvpScene) {
          m_Value = GetWinUserId(scene);
        } else {
          m_Value = 0;
        }
      }
    }

    private int GetWinUserId(Scene scene)
    {
      Room room = scene.GetRoom();
      UserInfo one = null, two = null;
      LinkedListNode<UserInfo> node = scene.UserManager.Users.FirstValue;
      if (null != node) {
        one = node.Value;
        node = node.Next;
        if (null != node) {
          two = node.Value;
        }
      }
      int winUserId = 0;
      if (null != one) {
        if (null != two) {
          float maxHpOne = one.GetActualProperty().HpMax;
          float maxHpTwo = two.GetActualProperty().HpMax;
          if (one.Hp / maxHpOne >= two.Hp / maxHpTwo) {
            winUserId = one.GetCampId();
          } else {
            winUserId = two.GetCampId();
          }
        } else {
          winUserId = one.GetCampId();
        }
      } else if (null != two) {
        winUserId = two.GetCampId();
      }
      return winUserId;
    }

    private object m_Iterator = null;
    private object[] m_Args = null;

    private bool m_HaveValue;
    private object m_Value;
  }
  internal sealed class LostUserIdValue : IStoryValue<object>
  {
    public void InitFromDsl(ScriptableData.ISyntaxComponent param)
    {
      ScriptableData.CallData callData = param as ScriptableData.CallData;
      if (null != callData && callData.GetId() == "lostuserid") {
      }
    }
    public IStoryValue<object> Clone()
    {
      LostUserIdValue val = new LostUserIdValue();
      val.m_HaveValue = m_HaveValue;
      val.m_Value = m_Value;
      return val;
    }
    public void Evaluate(object iterator, object[] args)
    {
      m_Iterator = iterator;
      m_Args = args;
    }
    public void Evaluate(StoryInstance instance)
    {
      TryUpdateValue(instance);
    }
    public void Analyze(StoryInstance instance)
    {
    }
    public bool HaveValue
    {
      get
      {
        return m_HaveValue;
      }
    }
    public object Value
    {
      get
      {
        return m_Value;
      }
    }

    private void TryUpdateValue(StoryInstance instance)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        m_HaveValue = true;
        if (scene.IsPvpScene) {
          m_Value = GetLostUserId(scene);
        } else {
          m_Value = 0;
        }
      }
    }

    private int GetLostUserId(Scene scene)
    {
      Room room = scene.GetRoom();
      UserInfo one = null, two = null;
      LinkedListNode<UserInfo> node = scene.UserManager.Users.FirstValue;
      if (null != node) {
        one = node.Value;
        node = node.Next;
        if (null != node) {
          two = node.Value;
        }
      }
      int lostUserId = 0;
      if (null != one) {
        if (null != two) {
          float maxHpOne = one.GetActualProperty().HpMax;
          float maxHpTwo = two.GetActualProperty().HpMax;
          if (one.Hp / maxHpOne >= two.Hp / maxHpTwo) {
            lostUserId = two.GetCampId();
          } else {
            lostUserId = one.GetCampId();
          }
        }
      }
      return lostUserId;
    }

    private object m_Iterator = null;
    private object[] m_Args = null;

    private bool m_HaveValue;
    private object m_Value;
  }
  internal sealed class LivingUserCountValue : IStoryValue<object>
  {
    public void InitFromDsl(ScriptableData.ISyntaxComponent param)
    {
      ScriptableData.CallData callData = param as ScriptableData.CallData;
      if (null != callData && callData.GetId() == "livingusercount") {
      }
    }
    public IStoryValue<object> Clone()
    {
      LivingUserCountValue val = new LivingUserCountValue();
      val.m_HaveValue = m_HaveValue;
      val.m_Value = m_Value;
      return val;
    }
    public void Evaluate(object iterator, object[] args)
    {
      m_Iterator = iterator;
      m_Args = args;
    }
    public void Evaluate(StoryInstance instance)
    {
      TryUpdateValue(instance);
    }
    public void Analyze(StoryInstance instance)
    {
    }
    public bool HaveValue
    {
      get
      {
        return m_HaveValue;
      }
    }
    public object Value
    {
      get
      {
        return m_Value;
      }
    }

    private void TryUpdateValue(StoryInstance instance)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        m_HaveValue = true;
        m_Value = scene.GetLivingUserCount();
      }
    }

    private object m_Iterator = null;
    private object[] m_Args = null;

    private bool m_HaveValue;
    private object m_Value;
  }
}
