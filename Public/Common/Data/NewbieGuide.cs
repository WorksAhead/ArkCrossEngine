using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class NewbieGuideConfig : IData
    {
        public int Id = 0;
        public int m_TriggerLevelMini = 0;
        public int m_TriggerLevelMax = 1000;//新手引导最大等级限制
        public int m_TriggerSceneId = -1;//触发新手引导需要完成的关卡Id
        public int m_GroupId = -1;
        public int m_GroupIndex = -1;
        public int m_TriggerUiId;
        public int m_ResetToGuideId;
        public int m_TargetChildIndex;
        public string m_TargetChildPath = "";
        public bool m_NeedGuideDlg;//是否需要对话框
        public bool m_AlwaysNeedGuideDlg;//是否每次都需要显示对话
        public bool m_IsSpeakerAtLeft;
        public string m_Words;
        public float[] m_RelativeScreenPos = new float[2];
        public string m_GuideUiPath = "";
        //历史遗留参数，已废弃
        public float[] m_RotateThree = { 0.0f, 0.0f, 0.0f };
        public bool m_Visible = false;
        public int m_Type = 0;
        public int m_PreviousGuideId = 0;
        public float[] m_LocalPosition = { 0.0f, 0.0f, 0.0f };
        public int m_ChildNumber = 0;
        public string m_ChildName = "";
        public float[] m_Scale = { 1.0f, 1.0f, 1.0f };

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            m_TriggerLevelMini = DBCUtil.ExtractNumeric<int>(node, "TriggerLevelMini", 100000, false);
            m_TriggerLevelMax = DBCUtil.ExtractNumeric<int>(node, "TriggerLevelMax", 100000, false);
            m_TriggerSceneId = DBCUtil.ExtractNumeric<int>(node, "TriggerSceneId", -1, false);
            m_GroupId = DBCUtil.ExtractNumeric<int>(node, "GroupId", -1, false);
            m_GroupIndex = DBCUtil.ExtractNumeric<int>(node, "GroupIndex", -1, false);
            m_ResetToGuideId = DBCUtil.ExtractNumeric<int>(node, "ResetToGuideId", -1, false);
            m_TriggerUiId = DBCUtil.ExtractNumeric<int>(node, "TriggerUiId", -1, false);
            m_TargetChildPath = DBCUtil.ExtractString(node, "TargetChildPath", "", true);
            m_GuideUiPath = DBCUtil.ExtractString(node, "GuideUiPath", "", true);
            m_TargetChildIndex = DBCUtil.ExtractNumeric<int>(node, "TargetChildIndex", -1, false);
            m_NeedGuideDlg = DBCUtil.ExtractBool(node, "NeedGuideDlg", true, false);
            m_AlwaysNeedGuideDlg = DBCUtil.ExtractBool(node, "AlwaysNeedGuideDlg", false, false);
            m_IsSpeakerAtLeft = DBCUtil.ExtractBool(node, "IsSpeakerAtLeft", true, false);
            m_Words = DBCUtil.ExtractString(node, "Words", "", false);
            List<float> RelativePos = DBCUtil.ExtractNumericList<float>(node, "ScreenPosition", 0, false);
            if (RelativePos.Count >= 2)
            {
                m_RelativeScreenPos[0] = RelativePos[0];
                m_RelativeScreenPos[1] = RelativePos[1];
            }
            List<float> list = DBCUtil.ExtractNumericList<float>(node, "Rotate", 0, false);
            int num = list.Count;
            if (num > 0) m_RotateThree[0] = list[0];
            if (num > 1) m_RotateThree[1] = list[1];
            if (num > 2) m_RotateThree[2] = list[2];

            m_Visible = DBCUtil.ExtractNumeric<bool>(node, "Visible", false, false);
            m_Type = DBCUtil.ExtractNumeric<int>(node, "Type", 0, true);
            m_PreviousGuideId = DBCUtil.ExtractNumeric<int>(node, "PreviousGuideId", 0, true);

            list = DBCUtil.ExtractNumericList<float>(node, "LocalPosition", 0, false);
            num = list.Count;
            if (num > 0) m_LocalPosition[0] = list[0];
            if (num > 1) m_LocalPosition[1] = list[1];
            if (num > 2) m_LocalPosition[2] = list[2];

            m_ChildNumber = DBCUtil.ExtractNumeric<int>(node, "ChildNumber", 0, true);
            m_ChildName = DBCUtil.ExtractString(node, "ChildName", "", true);

            list = DBCUtil.ExtractNumericList<float>(node, "Scale", 0, false);
            num = list.Count;
            if (num > 0) m_Scale[0] = list[0];
            if (num > 1) m_Scale[1] = list[1];
            if (num > 2) m_Scale[2] = list[2];
            return true;
        }
        public int GetId()
        {
            return Id;
        }
    }
    public class NewbieGuideProvider
    {
        public DataDictionaryMgr<NewbieGuideConfig> NewbieGuideMgr
        {
            get { return m_NewbieGuideMgr; }
        }
        public NewbieGuideConfig GetDataById(int id)
        {
            return m_NewbieGuideMgr.GetDataById(id);
        }
        public List<NewbieGuideConfig> GetAllConfig()
        {
            List<NewbieGuideConfig> newbieGuideList = new List<NewbieGuideConfig>();
            if (newbieGuideList == null) return null;
            MyDictionary<int, object> dicts = m_NewbieGuideMgr.GetData();
            foreach (NewbieGuideConfig cfg in dicts.Values)
            {
                newbieGuideList.Add(cfg);
            }
            return newbieGuideList;
        }
        public int GetDataCount()
        {
            return m_NewbieGuideMgr.GetDataCount();
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_NewbieGuideMgr.CollectDataFromDBC(file, root, bytes);
        }

        private DataDictionaryMgr<NewbieGuideConfig> m_NewbieGuideMgr = new DataDictionaryMgr<NewbieGuideConfig>();

        public static NewbieGuideProvider Instance
        {
            get { return s_Instance; }
        }
        private static NewbieGuideProvider s_Instance = new NewbieGuideProvider();
    }
}
