using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum PartnerMsgResultEnum
    {
        SUCCESS = 0,
        FAILED,
        DEMOTION,
        NEED_MORE_GOLD,
        NEED_MORE_ITEM,
        MAX_LEVEL,
        ERROR,
        MAX,
    }
    public class PartnerStateInfo
    {
        public void Reset()
        {
            m_CurActivePartner = null;
            m_IsPartnerPlayed = false;
            m_PartnerList.Clear();
        }
        public int GetActivePartnerId()
        {
            if (null != m_CurActivePartner)
            {
                return m_CurActivePartner.Id;
            }
            else
            {
                return -1;
            }
        }
        public PartnerInfo GetActivePartner()
        {
            return m_CurActivePartner;
        }
        public void SetActivePartner(int id)
        {
            m_CurActivePartner = GetPartnerInfoById(id);
        }
        public List<PartnerInfo> GetAllPartners()
        {
            return m_PartnerList;
        }
        public bool IsHavePartner(int id)
        {
            return null != GetPartnerInfoById(id);
        }
        public void AddPartner(int partnerId, int skillStage, int additionLevel)
        {
            lock (m_Lock)
            {
                foreach (PartnerInfo pi in m_PartnerList)
                {
                    if (pi.Id == partnerId)
                    {
                        pi.CurAdditionLevel = additionLevel;
                        pi.CurSkillStage = skillStage;
                        return;
                    }
                }
                PartnerInfo partnerInfo = CreatePartnerInfo(partnerId);
                if (null != partnerInfo && !m_PartnerList.Contains(partnerInfo))
                {
                    m_PartnerList.Add(partnerInfo);
                    partnerInfo.CurSkillStage = skillStage;
                    partnerInfo.CurAdditionLevel = additionLevel;
                }
            }
        }
        public void SetPartnerLevel(int id, int level)
        {
            PartnerInfo info = GetPartnerInfoById(id);
            if (null != info)
            {
                info.CurAdditionLevel = level;
            }
        }
        public void UpgradePartnerLevel(int id)
        {
            PartnerInfo info = GetPartnerInfoById(id);
            if (null != info)
            {
                ++info.CurAdditionLevel;
            }
        }
        public void SetPartnerStage(int id, int level)
        {
            PartnerInfo info = GetPartnerInfoById(id);
            if (null != info)
            {
                info.CurSkillStage = level;
            }
        }
        public void UpgradePartnerStage(int id)
        {
            PartnerInfo info = GetPartnerInfoById(id);
            if (null != info)
            {
                ++info.CurSkillStage;
            }
        }
        public bool IsPartnerPlayed
        {
            get { return m_IsPartnerPlayed; }
            set { m_IsPartnerPlayed = value; }
        }
        public float ActivePartnerHpPercent
        {
            get { return m_ActivePartnerHpPercent; }
            set { m_ActivePartnerHpPercent = value; }
        }
        public PartnerInfo GetPartnerInfoById(int id)
        {
            lock (m_Lock)
            {
                return m_PartnerList.Find(info => info.Id == id);
            }
        }
        public object Lock
        {
            get { return m_Lock; }
        }
        private PartnerInfo CreatePartnerInfo(int partnerId)
        {
            PartnerInfo result = null;
            PartnerConfig config = PartnerConfigProvider.Instance.GetDataById(partnerId);
            if (null != config)
            {
                result = new PartnerInfo(config);
            }
            return result;
        }
        private object m_Lock = new object();
        private bool m_IsPartnerPlayed = false;
        private float m_ActivePartnerHpPercent = 1.0f;
        private PartnerInfo m_CurActivePartner = null;
        private List<PartnerInfo> m_PartnerList = new List<PartnerInfo>();
    }
}
