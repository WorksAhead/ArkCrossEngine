using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum WeaponActionSectionNum
    {
        ASN_PREFIRE = 0,
        ASN_FIRE = 1,
        ASN_ENDFIRE = 2,
    }
    public class Data_ActionConfig : IData
    {
        public class Data_ActionInfo
        {
            public string m_AnimName;
            public string m_SoundId;
        }

        public int m_ModelId;
        public Data_ActionInfo m_Default;
        public Dictionary<Animation_Type, List<Data_ActionInfo>> m_ActionContainer;
        public Dictionary<Animation_Type, string> m_ActionAnimNameContainer;

        public float m_CombatStdSpeed;
        public float m_ForwardStdSpeed;
        public float m_SlowStdSpeed;
        public float m_FastStdSpeed;
        public string m_ActionPrefix;

        private void AddActionAnimName(Animation_Type at)
        {
            if (m_ActionContainer[at].Count > 0)
            {
                m_ActionAnimNameContainer[at] = m_ActionContainer[at][0].m_AnimName;
            }
        }
        /**
         * @brief 提取数据
         *
         * @param node
         *
         * @return 
         */
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_ModelId = DBCUtil.ExtractNumeric<int>(node, "ModelId", 0, false);
            m_CombatStdSpeed = DBCUtil.ExtractNumeric<float>(node, "CombatStdSpeed", 3.0f, false);
            m_ForwardStdSpeed = DBCUtil.ExtractNumeric<float>(node, "ForwardStdSpeed", 3.0f, true);
            m_SlowStdSpeed = DBCUtil.ExtractNumeric<float>(node, "SlowStdSpeed", 3.0f, false);
            m_FastStdSpeed = DBCUtil.ExtractNumeric<float>(node, "FastStdSpeed", 3.0f, false);

            m_ActionPrefix = DBCUtil.ExtractString(node, "ActionPrefix", "", false);

            m_ActionContainer = new Dictionary<Animation_Type, List<Data_ActionInfo>>();
            m_ActionAnimNameContainer = new Dictionary<Animation_Type, string>();

            m_ActionContainer[Animation_Type.AT_SLEEP] = ExtractAction(node, "Sleep");
            AddActionAnimName(Animation_Type.AT_SLEEP);

            m_ActionContainer[Animation_Type.AT_Stand] = ExtractAction(node, "Stand_0");
            AddActionAnimName(Animation_Type.AT_Stand);

            m_ActionContainer[Animation_Type.AT_IdelStand] = ExtractAction(node, "Idle_Stand");
            AddActionAnimName(Animation_Type.AT_IdelStand);
            m_ActionContainer[Animation_Type.AT_Idle0] = ExtractAction(node, "Idle_0");
            AddActionAnimName(Animation_Type.AT_Idle0);
            m_ActionContainer[Animation_Type.AT_Idle1] = ExtractAction(node, "Idle_1");
            AddActionAnimName(Animation_Type.AT_Idle1);
            m_ActionContainer[Animation_Type.AT_Idle2] = ExtractAction(node, "Idle_2");
            AddActionAnimName(Animation_Type.AT_Idle2);
            m_ActionContainer[Animation_Type.AT_FlyUp] = ExtractAction(node, "FlyUp");
            AddActionAnimName(Animation_Type.AT_FlyUp);
            m_ActionContainer[Animation_Type.AT_FlyDown] = ExtractAction(node, "FlyDown");
            AddActionAnimName(Animation_Type.AT_FlyDown);
            m_ActionContainer[Animation_Type.AT_FlyHurt] = ExtractAction(node, "FlyHurt");
            AddActionAnimName(Animation_Type.AT_FlyHurt);
            m_ActionContainer[Animation_Type.AT_FlyDownGround] = ExtractAction(node, "FlyDownGround");
            AddActionAnimName(Animation_Type.AT_FlyDownGround);
            m_ActionContainer[Animation_Type.AT_OnGround] = ExtractAction(node, "OnGround");
            AddActionAnimName(Animation_Type.AT_OnGround);
            m_ActionContainer[Animation_Type.AT_Grab] = ExtractAction(node, "Grab");
            AddActionAnimName(Animation_Type.AT_Grab);

            m_ActionContainer[Animation_Type.AT_CombatStand] = ExtractAction(node, "CombatStand");
            AddActionAnimName(Animation_Type.AT_CombatStand);
            m_ActionContainer[Animation_Type.AT_CombatRun] = ExtractAction(node, "CombatRun");
            AddActionAnimName(Animation_Type.AT_CombatRun);

            m_ActionContainer[Animation_Type.AT_SlowMove] = ExtractAction(node, "SlowMove");
            AddActionAnimName(Animation_Type.AT_SlowMove);
            m_ActionContainer[Animation_Type.AT_FastMove] = ExtractAction(node, "FastMove");
            AddActionAnimName(Animation_Type.AT_FastMove);
            m_ActionContainer[Animation_Type.AT_RunForward] = ExtractAction(node, "RunForward_0");
            AddActionAnimName(Animation_Type.AT_RunForward);

            m_ActionContainer[Animation_Type.AT_Born] = ExtractAction(node, "Born");
            AddActionAnimName(Animation_Type.AT_Born);

            m_ActionContainer[Animation_Type.AT_Hurt0] = ExtractAction(node, "Hurt_0");
            AddActionAnimName(Animation_Type.AT_Hurt0);
            m_ActionContainer[Animation_Type.AT_Hurt1] = ExtractAction(node, "Hurt_1");
            AddActionAnimName(Animation_Type.AT_Hurt1);
            m_ActionContainer[Animation_Type.AT_Hurt2] = ExtractAction(node, "Hurt_2");
            AddActionAnimName(Animation_Type.AT_Hurt2);
            m_ActionContainer[Animation_Type.AT_Dead] = ExtractAction(node, "Dead");
            AddActionAnimName(Animation_Type.AT_Dead);
            m_ActionContainer[Animation_Type.AT_Taunt] = ExtractAction(node, "Taunt");
            AddActionAnimName(Animation_Type.AT_Taunt);
            m_ActionContainer[Animation_Type.AT_PostDead] = ExtractAction(node, "PostDead");
            AddActionAnimName(Animation_Type.AT_PostDead);


            m_ActionContainer[Animation_Type.AT_GetUp1] = ExtractAction(node, "GetUp_0");
            AddActionAnimName(Animation_Type.AT_GetUp1);
            m_ActionContainer[Animation_Type.AT_GetUp2] = ExtractAction(node, "GetUp_1");
            AddActionAnimName(Animation_Type.AT_GetUp2);

            m_ActionContainer[Animation_Type.AT_Celebrate] = ExtractAction(node, "Celebrate_0");
            AddActionAnimName(Animation_Type.AT_Celebrate);
            m_ActionContainer[Animation_Type.AT_Depressed] = ExtractAction(node, "Depressed_0");
            AddActionAnimName(Animation_Type.AT_Depressed);
            m_ActionContainer[Animation_Type.AT_SkillSection1] = ExtractAction(node, "SkillSection01");
            AddActionAnimName(Animation_Type.AT_SkillSection1);
            m_ActionContainer[Animation_Type.AT_SkillSection2] = ExtractAction(node, "SkillSection02");
            AddActionAnimName(Animation_Type.AT_SkillSection2);
            m_ActionContainer[Animation_Type.AT_SkillSection3] = ExtractAction(node, "SkillSection03");
            AddActionAnimName(Animation_Type.AT_SkillSection3);
            m_ActionContainer[Animation_Type.AT_SkillSection4] = ExtractAction(node, "SkillSection04");
            AddActionAnimName(Animation_Type.AT_SkillSection4);
            m_ActionContainer[Animation_Type.AT_SkillSection5] = ExtractAction(node, "SkillSection05");
            AddActionAnimName(Animation_Type.AT_SkillSection5);
            m_ActionContainer[Animation_Type.AT_SkillSection6] = ExtractAction(node, "SkillSection06");
            AddActionAnimName(Animation_Type.AT_SkillSection6);
            m_ActionContainer[Animation_Type.AT_SkillSection7] = ExtractAction(node, "SkillSection07");
            AddActionAnimName(Animation_Type.AT_SkillSection7);
            m_ActionContainer[Animation_Type.AT_SkillSection8] = ExtractAction(node, "SkillSection08");
            AddActionAnimName(Animation_Type.AT_SkillSection8);
            m_ActionContainer[Animation_Type.AT_SkillSection9] = ExtractAction(node, "SkillSection09");
            AddActionAnimName(Animation_Type.AT_SkillSection9);
            m_ActionContainer[Animation_Type.AT_SkillSection10] = ExtractAction(node, "SkillSection10");
            AddActionAnimName(Animation_Type.AT_SkillSection10);
            m_ActionContainer[Animation_Type.AT_SkillSection11] = ExtractAction(node, "SkillSection11");
            AddActionAnimName(Animation_Type.AT_SkillSection11);
            m_ActionContainer[Animation_Type.AT_SkillSection12] = ExtractAction(node, "SkillSection12");
            AddActionAnimName(Animation_Type.AT_SkillSection12);
            m_ActionContainer[Animation_Type.AT_SkillSection13] = ExtractAction(node, "SkillSection13");
            AddActionAnimName(Animation_Type.AT_SkillSection13);
            m_ActionContainer[Animation_Type.AT_SkillSection14] = ExtractAction(node, "SkillSection14");
            AddActionAnimName(Animation_Type.AT_SkillSection14);
            m_ActionContainer[Animation_Type.AT_SkillSection15] = ExtractAction(node, "SkillSection15");
            AddActionAnimName(Animation_Type.AT_SkillSection15);
            m_ActionContainer[Animation_Type.AT_SkillSection16] = ExtractAction(node, "SkillSection16");
            AddActionAnimName(Animation_Type.AT_SkillSection16);
            m_ActionContainer[Animation_Type.AT_SkillSection17] = ExtractAction(node, "SkillSection17");
            AddActionAnimName(Animation_Type.AT_SkillSection17);
            m_ActionContainer[Animation_Type.AT_SkillSection18] = ExtractAction(node, "SkillSection18");
            AddActionAnimName(Animation_Type.AT_SkillSection18);
            m_ActionContainer[Animation_Type.AT_SkillSection19] = ExtractAction(node, "SkillSection19");
            AddActionAnimName(Animation_Type.AT_SkillSection19);
            m_ActionContainer[Animation_Type.AT_SkillSection20] = ExtractAction(node, "SkillSection20");
            AddActionAnimName(Animation_Type.AT_SkillSection20);
            return true;
        }

        /**
         * @brief 获取数据ID
         *
         * @return 
         */
        public virtual int GetId()
        {
            return m_ModelId;
        }

        /**
         * @brief 获取动作数量
         *
         * @return 
         */
        public int GetActionCountByType(Animation_Type type)
        {
            List<Data_ActionInfo> infos;
            if (!m_ActionContainer.TryGetValue(type, out infos))
            {
                return 0;
            }
            return infos.Count;
        }

        /**
         * @brief 获取随机动作
         *
         * @return 
         */
        public Data_ActionInfo GetRandomActionByType(Animation_Type type)
        {
            int count = GetActionCountByType(type);
            if (count > 0)
            {
                int randIndex = CrossEngineHelper.Random.Next(count);
                return m_ActionContainer[type][randIndex];
            }

            return null;
        }

        public List<Data_ActionInfo> GetAllActionByType(Animation_Type type)
        {
            List<Data_ActionInfo> infos;
            if (m_ActionContainer.TryGetValue(type, out infos))
            {
                return infos;
            }
            else
            {
                return new List<Data_ActionInfo>();
            }
        }

        /**
         * @brief 私有提取函数
         *
         * @return 
         */
        private List<Data_ActionInfo> ExtractAction(DBC_Row node, string prefix)
        {
            List<Data_ActionInfo> data = new List<Data_ActionInfo>();

            List<string> childList = DBCUtil.ExtractNodeByPrefix(node, prefix);

            if (childList.Count == 0)
                return data;

            foreach (string child in childList)
            {
                if (string.IsNullOrEmpty(child))
                {
                    continue;
                }

                string outModelPath;
                string outSoundId;
                if (!_ParseModelPath(child, out outModelPath, out outSoundId))
                {
                    string info = "[Err]:ActionConfigProvider.ExtractAction anim name error:" + child;
                    throw new Exception(info);
                }

                Data_ActionInfo action = new Data_ActionInfo();
                action.m_AnimName = m_ActionPrefix + outModelPath;
                action.m_SoundId = outSoundId;
                data.Add(action);
            }

            return data;
        }

        private static bool _ParseModelPath(string path, out string outModelPath, out string outSoundId)
        {
            string[] resut = path.Split(new String[] { "@" }, StringSplitOptions.None);
            if (resut != null)
            {
                outModelPath = (resut.Length > 0) ? resut[0] : "";
                outSoundId = (resut.Length > 1) ? resut[1] : "";
            }
            else
            {
                outModelPath = "";
                outSoundId = "";
            }
            return true;
        }
    }

    public class ActionConfigProvider
    {
        public DataDictionaryMgr<Data_ActionConfig> ActionConfigMgr
        {
            get { return m_ActionConfigMgr; }
        }
        public Data_ActionConfig GetDataById(int id)
        {
            return m_ActionConfigMgr.GetDataById(id);
        }

        public Data_ActionConfig GetCharacterCurActionConfig(List<int> action_list)
        {
            for (int i = 0; i < action_list.Count; ++i)
            {
                Data_ActionConfig action_config = GetDataById(action_list[i]);
                if (action_config != null)
                {
                    return action_config;
                }
            }
            return null;
        }

        public void Load(string file, string root, byte[] bytes)
        {
            m_ActionConfigMgr.CollectDataFromDBC(file, root, bytes);
        }

        private DataDictionaryMgr<Data_ActionConfig> m_ActionConfigMgr = new DataDictionaryMgr<Data_ActionConfig>();

        public static ActionConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static ActionConfigProvider s_Instance = new ActionConfigProvider();
    }
}
