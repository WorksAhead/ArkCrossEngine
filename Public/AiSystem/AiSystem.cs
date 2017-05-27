using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum AiEventEnum
    {
        DEAD = 0,
        CONTROL_TIME_OVERFLOW,
        GET_UP,
    }
    public sealed class AiSystem
    {
        public AiCommandDispatcher AiCommandDispatcher
        {
            get { return m_AiCommandDispatcher; }
        }
        public void Reset()
        {
            m_LastTickTime = 0;
        }
        public void SetNpcManager(NpcManager npcMgr)
        {
            m_NpcMgr = npcMgr;
        }
        public void SetUserManager(UserManager userMgr)
        {
            m_UserMgr = userMgr;
        }
        public void Tick()
        {
            if (0 == m_LastTickTime)
            {
                m_LastTickTime = TimeUtility.GetServerMilliseconds();
            }
            else
            {
                long delta = TimeUtility.GetServerMilliseconds() - m_LastTickTime;
                m_LastTickTime = TimeUtility.GetServerMilliseconds();
                if (null != m_NpcMgr)
                {
                    for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        NpcInfo npc = linkNode.Value;
                        TickNpc(npc, delta);
                    }
                }
                if (null != m_UserMgr)
                {
                    for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo user = linkNode.Value;
                        TickUser(user, delta);
                    }
                }
            }
        }
        public void AlwaysClientTick()
        {
            if (0 == m_AlwaysClientLastTickTime)
            {
                m_AlwaysClientLastTickTime = TimeUtility.GetServerMilliseconds();
            }
            else
            {
                long delta = TimeUtility.GetServerMilliseconds() - m_AlwaysClientLastTickTime;
                m_AlwaysClientLastTickTime = TimeUtility.GetServerMilliseconds();
                if (null != m_NpcMgr)
                {
                    for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        NpcInfo npc = linkNode.Value;
                        INpcStateLogic logic = AiLogicManager.Instance.GetAlwaysClientNpcStateLogic(npc.GetAiStateInfo().AiLogic);
                        if (null != logic)
                        {
                            logic.Execute(npc, m_AiCommandDispatcher, delta);
                        }
                    }
                }
                if (null != m_UserMgr)
                {
                    for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo user = linkNode.Value;
                        IUserStateLogic logic = AiLogicManager.Instance.GetAlwaysClientUserStateLogic(user.GetAiStateInfo().AiLogic);
                        if (null != logic)
                        {
                            logic.Execute(user, m_AiCommandDispatcher, delta);
                        }
                    }
                }
            }
        }

        private void TickNpc(NpcInfo npc, long delta)
        {
            INpcStateLogic logic = AiLogicManager.Instance.GetNpcStateLogic(npc.GetAiStateInfo().AiLogic);
            if (null != logic)
            {
                logic.Execute(npc, m_AiCommandDispatcher, delta);
                logic.UpdateTarget(npc);
            }
        }

        private void TickUser(UserInfo user, long delta)
        {
            IUserStateLogic logic = AiLogicManager.Instance.GetUserStateLogic(user.GetAiStateInfo().AiLogic);
            if (null != logic)
            {
                logic.Execute(user, m_AiCommandDispatcher, delta);
            }
        }

        private long m_LastTickTime = 0;
        private long m_AlwaysClientLastTickTime = 0;
        private NpcManager m_NpcMgr = null;
        private UserManager m_UserMgr = null;

        private AiCommandDispatcher m_AiCommandDispatcher = new AiCommandDispatcher();
    }
}
