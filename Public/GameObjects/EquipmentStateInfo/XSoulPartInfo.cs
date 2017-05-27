using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class XSoulPartInfo
    {
        public XSoulPartInfo(XSoulPart part, ItemDataInfo item)
        {
            m_Part = part;
            m_XSoulPartItem = item;
        }

        public ItemDataInfo XSoulPartItem
        {
            get { return m_XSoulPartItem; }
            set { m_XSoulPartItem = value; }
        }

        public bool IsEquipModelChanged()
        {
            LogSystem.Debug("----xsoul levelmodel={0}, curmodel={1}, curshowlevel={2}", GetLevelModel(), m_CurShowedEquip, m_ShowModelLevel);
            if (GetLevelModel().Equals(m_CurShowedEquip))
            {
                return false;
            }
            return true;
        }

        public bool IsLevelChanged
        {
            set { m_IsLevelChanged = value; }
            get { return m_IsLevelChanged; }
        }

        public int ShowModelLevel
        {
            set { m_ShowModelLevel = value; }
            get { return m_ShowModelLevel; }
        }

        public void UpdateXSoulExperience(int experience)
        {
            int old_level = m_XSoulPartItem.Level;
            m_XSoulPartItem.Experience = experience;
            m_XSoulPartItem.UpdateLevelByExperience();
            if (m_XSoulPartItem.Level != old_level)
            {
                m_IsLevelChanged = true;
                m_ShowModelLevel = -1;
            }
        }

        public List<int> GetActiveImpacts()
        {
            List<int> result = new List<int>();
            int level = m_XSoulPartItem.Level;
            ItemConfig config = ItemConfigProvider.Instance.GetDataById(m_XSoulPartItem.ItemId);
            if (config == null || config.m_ActiveBuffOnLevel == null)
            {
                LogSystem.Debug("---ActiveImpactOnLevel is not configured!");
                return result;
            }
            for (int i = 0; i + 1 < config.m_ActiveBuffOnLevel.Length; i += 2)
            {
                if (config.m_ActiveBuffOnLevel[i] <= level)
                {
                    result.Add(config.m_ActiveBuffOnLevel[i + 1]);
                }
            }
            return result;
        }

        public void SetCurShowedModel(string model)
        {
            m_CurShowedEquip = model;
        }

        public string GetWearNodeAndName()
        {
            if (m_XSoulPartItem == null)
            {
                return "";
            }
            ItemConfig config = ItemConfigProvider.Instance.GetDataById(m_XSoulPartItem.ItemId);
            if (config == null)
            {
                return "";
            }
            return config.m_WearNodeAndName;
        }

        public string GetLevelModel()
        {
            if (m_XSoulPartItem == null)
            {
                return "";
            }
            ItemConfig config = ItemConfigProvider.Instance.GetDataById(m_XSoulPartItem.ItemId);
            if (config == null)
            {
                return "";
            }
            if (m_ShowModelLevel > 0)
            {
                return GetLevelModelImpl(m_ShowModelLevel, config);
            }
            else
            {
                return GetLevelModelImpl(m_XSoulPartItem.Level, config);
            }
        }

        private string GetLevelModelImpl(int level, ItemConfig config)
        {
            string target_model = "";
            int fit_model_level = -1;
            if (config.m_UseModelOnLevel == null)
            {
                LogSystem.Debug("---UseModelOnLevel is not configured!");
                return target_model;
            }
            for (int i = 0; i < config.m_UseModelOnLevel.Length; i++)
            {
                string[] model_info = config.m_UseModelOnLevel[i].Split(':');
                if (model_info.Length < 2)
                {
                    continue;
                }
                int model_level = int.Parse(model_info[0]);
                if (model_level <= level && model_level > fit_model_level)
                {
                    fit_model_level = model_level;
                    target_model = model_info[1];
                }
            }
            return target_model;
        }

        private XSoulPart m_Part;
        private ItemDataInfo m_XSoulPartItem;
        private string m_CurShowedEquip = "";
        private bool m_IsLevelChanged;
        private int m_ShowModelLevel = -1;
    }
}
