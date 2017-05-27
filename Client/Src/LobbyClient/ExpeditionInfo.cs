using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class ExpeditionImageInfo
    {
        public ExpeditionImageInfo()
        {
        }
        public ulong Guid
        {
            get { return m_Guid; }
            set { m_Guid = value; }
        }
        public int HeroId
        {
            get { return m_HeroId; }
            set { m_HeroId = value; }
        }
        public string Nickname
        {
            get { return m_Nickname; }
            set { m_Nickname = value; }
        }
        public int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        public int FightingScore
        {
            get { return m_FightingScore; }
            set { m_FightingScore = value; }
        }
        public ItemDataInfo[] Equips
        {
            get { return m_EquipInfo; }
        }
        public List<SkillInfo> Skills
        {
            get { return m_SkillInfo; }
        }
        public ItemDataInfo[] Legacys
        {
            get { return m_LegacyInfo; }
        }
        private ulong m_Guid = 0;
        private int m_HeroId = 0;
        private string m_Nickname = "";
        private int m_Level = 1;
        private int m_FightingScore = 0;
        private ItemDataInfo[] m_EquipInfo = new ItemDataInfo[EquipmentStateInfo.c_EquipmentCapacity];
        private List<SkillInfo> m_SkillInfo = new List<SkillInfo>();
        private ItemDataInfo[] m_LegacyInfo = new ItemDataInfo[LegacyStateInfo.c_LegacyCapacity];
    }
    public enum EnemyType : int
    {
        ET_Monster = 0,
        ET_Boss,
        ET_OnePlayer,
        ET_TwoPlayer,
    }
    public class MatchExpeditionArg
    {
        public MatchExpeditionArg(int id, int attr)
        {
            this.Id = id;
            this.Attr = attr;
        }
        public int Id
        {
            get;
            set;
        }
        public int Attr
        {
            get;
            set;
        }
    }
    public class ExpeditionPlayerInfo
    {
        public static void CaclCoefficient(UserInfo one, UserInfo two)
        {
            float lvl1 = one.GetLevel();
            float lvl2 = two.GetLevel();
            float lvl = (lvl1 + lvl2) / 2;
            double c = 4.09 * 1.2 * (1 + lvl * 0.04) * (1 + (0.15 * (1.62 - 1) / 50 * lvl) + (1.05 + 0.55 / 50 * lvl - 1) * 0.5 + (1.05 + 0.55 / 50 * lvl - 1) * 0.5);
            one.HpMaxCoefficient = (float)c;
            two.HpMaxCoefficient = (float)c;
            one.EnergyMaxCoefficient = (float)c;
            two.EnergyMaxCoefficient = (float)c;
        }
        const int c_MaxExpeditionNum = 12;
        public class AwardItemData
        {
            public AwardItemData()
            {
                this.m_ItemId = 0;
                this.m_ItemNum = 0;
            }
            public int ItemId
            {
                get { return m_ItemId; }
                set { m_ItemId = value; }
            }
            public int ItemNum
            {
                get { return m_ItemNum; }
                set { m_ItemNum = value; }
            }
            private int m_ItemId;
            private int m_ItemNum;
        }
        public class TollgateData
        {
            public TollgateData()
            {
                Type = EnemyType.ET_Monster;
                this.m_FlushNum = -1;
                this.m_EnemyAttrList.Clear();
                this.m_EnemyList.Clear();
                this.m_UserImageList.Clear();
                this.m_IsFinish = false;
                this.m_IsPostResult = false;
                this.m_IsPlayAnim = false;
                this.m_IsDoClear = false;
                this.m_IsAcceptedAward = true;
                this.m_IsDelayRefreshed = false;
            }
            public void Reset()
            {
                Type = EnemyType.ET_Monster;
                this.m_FlushNum = -1;
                this.m_EnemyAttrList.Clear();
                this.m_EnemyList.Clear();
                this.m_UserImageList.Clear();
                this.m_IsFinish = false;
                this.m_IsPostResult = false;
                this.m_IsPlayAnim = false;
                this.m_IsDoClear = false;
                this.m_IsAcceptedAward = true;
                this.m_IsDelayRefreshed = false;
            }
            public EnemyType Type
            {
                get { return m_Type; }
                set
                {
                    m_Type = value;
                    if (EnemyType.ET_Monster == m_Type)
                    {
                        m_FlushNum = 2;
                    }
                    else
                    {
                        m_FlushNum = 1;
                    }
                }
            }
            public List<ExpeditionImageInfo> UserImageList
            {
                get { return m_UserImageList; }
                set { m_UserImageList = value; }
            }
            public List<int> EnemyList
            {
                get { return m_EnemyList; }
            }
            public List<int> EnemyAttrList
            {
                get { return m_EnemyAttrList; }
            }
            public bool IsFinish
            {
                get { return m_IsFinish; }
                set { m_IsFinish = value; }
            }
            public bool IsAcceptedAward
            {
                get { return m_IsAcceptedAward; }
                set { m_IsAcceptedAward = value; }
            }
            public int FlushNum
            {
                get { return m_FlushNum; }
                set { m_FlushNum = value; }
            }
            public bool IsPostResult
            {
                get { return m_IsPostResult; }
                set { m_IsPostResult = value; }
            }
            public bool IsPlayAnim
            {
                get { return m_IsPlayAnim; }
                set { m_IsPlayAnim = value; }
            }
            public bool IsDoClear
            {
                get { return m_IsDoClear; }
                set { m_IsDoClear = value; }
            }
            public bool IsDelayRefreshed
            {
                get { return m_IsDelayRefreshed; }
                set { m_IsDelayRefreshed = value; }
            }
            private EnemyType m_Type = EnemyType.ET_Monster;
            private List<int> m_EnemyList = new List<int>();
            private List<int> m_EnemyAttrList = new List<int>();
            private List<ExpeditionImageInfo> m_UserImageList = new List<ExpeditionImageInfo>();
            private bool m_IsFinish = false;
            private bool m_IsAcceptedAward = true;
            private int m_FlushNum = -1;
            private bool m_IsPostResult = false;
            private bool m_IsPlayAnim = false;
            private bool m_IsDoClear = false;
            private bool m_IsDelayRefreshed = false;
        }
        ///
        public ExpeditionPlayerInfo()
        {
            this.m_Hp = 1000;
            this.m_Mp = 1000;
            this.m_Rage = 0;
            this.m_Schedule = 0;
            this.m_StartTime = 0;
            this.m_MonsterDeadTime = -1;
            this.UserDeadTime = -1;

            for (int i = 0; i < m_Tollgates.Length; i++)
            {
                m_Tollgates[i] = new TollgateData();
            }
        }
        ///
        private int MatchMaxScore(int score, MonsterType type, out int match_score, out int match_attr)
        {
            match_attr = 0;
            match_score = 0;
            int min_monster_score = 10000;
            int min_monster_id = 0;
            for (int index = 0; index < ExpeditionMonsterConfigProvider.Instance.GetDataCount(); index++)
            {
                ExpeditionMonsterConfig monster_data = ExpeditionMonsterConfigProvider.Instance.GetDataById(index) as ExpeditionMonsterConfig;
                if (null != monster_data && type == monster_data.m_Type)
                {
                    int cur_score = monster_data.m_FightingScore;
                    if (cur_score < min_monster_score && cur_score > score)
                    {
                        match_score = cur_score;
                        min_monster_score = cur_score;
                        if (null != monster_data.m_LinkId && monster_data.m_LinkId.Count > 0
                          && null != monster_data.m_AttributeId && monster_data.m_AttributeId.Count > 0
                          && monster_data.m_LinkId.Count == monster_data.m_AttributeId.Count)
                        {
                            int md_ct = monster_data.m_LinkId.Count;
                            int rd_num = CrossEngineHelper.Random.Next(0, md_ct);
                            int link_id = monster_data.m_LinkId[rd_num];
                            int rd_attr = monster_data.m_AttributeId[rd_num];
                            if (link_id > 0)
                            {
                                min_monster_id = link_id;
                                match_attr = rd_attr;
                            }
                        }
                    }
                }
            }
            return min_monster_id;
        }
        public List<MatchExpeditionArg> MatchMonster(int score, MonsterType type)
        {
            List<MatchExpeditionArg> match_list = new List<MatchExpeditionArg>();
            if (MonsterType.MT_Boss == type)
            {
                int max_first_attr = 0;
                int out_max_match_score = 0;
                int max_first = MatchMaxScore(score, type, out out_max_match_score, out max_first_attr);
                MatchExpeditionArg max_first_obj = new MatchExpeditionArg(max_first, max_first_attr);
                match_list.Add(max_first_obj);
            }
            return match_list.Count > 0 ? match_list : null;
        }
        public void Reset()
        {
            this.m_Hp = 1000;
            this.m_Mp = 1000;
            this.m_Rage = 0;
            this.m_Schedule = 0;
            this.m_ActiveTollgate = 0;
            this.m_StartTime = 0;
            this.m_MonsterDeadTime = -1;
            this.UserDeadTime = -1;

            if (null != m_Tollgates)
            {
                for (int i = 0; i < m_Tollgates.Length; i++)
                {
                    if (null != m_Tollgates[i])
                    {
                        m_Tollgates[i].Reset();
                    }
                }
            }
        }
        public int Hp
        {
            get { return m_Hp; }
            set { m_Hp = value; }
        }
        public int Mp
        {
            get { return m_Mp; }
            set { m_Mp = value; }
        }
        public int Rage
        {
            get { return m_Rage; }
            set { m_Rage = value; }
        }
        public TollgateData[] Tollgates
        {
            get { return m_Tollgates; }
        }
        public int Schedule
        {
            get { return m_Schedule; }
            set { m_Schedule = value; }
        }
        public double ExpeditionResetIntervalTime
        {
            get
            {
                return c_ExpeditionResetIntervalTime;
            }
        }
        public double LastResetTimestamp
        {
            get { return m_LastResetTimestamp; }
            set { m_LastResetTimestamp = value; }
        }
        public int CurResetCount
        {
            get { return m_CurResetCount; }
            set { m_CurResetCount = value; }
        }
        public bool CanReset
        {
            get { return m_CanReset; }
            set { m_CanReset = value; }
        }
        public int ActiveTollgate
        {
            get { return m_ActiveTollgate; }
            set { m_ActiveTollgate = value; }
        }
        public bool IsUnlock
        {
            get { return m_IsUnlock; }
            set { m_IsUnlock = value; }
        }
        public double StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }
        public double MonsterDeadTime
        {
            get { return m_MonsterDeadTime; }
            set { m_MonsterDeadTime = value; }
        }
        public double UserDeadTime
        {
            get { return m_UserDeadTime; }
            set { m_UserDeadTime = value; }
        }
        public const double c_DelayFlushTime = 3.9;
        public const double c_ExpeditionResetIntervalTime = 10800;
        public const int c_FlushInterval = 7;
        public const int c_MixCoolTimePoint = 5;
        public const int c_UnlockLevel = 18;
        public const int c_ResetMax = 5;
        private bool m_IsUnlock = false;
        private int m_Hp;
        private int m_Mp;
        private int m_Rage;
        private int m_Schedule;
        private int m_CurResetCount = 0;
        private double m_LastResetTimestamp = 0;
        private bool m_CanReset = true;
        private int m_ActiveTollgate = 0;
        private double m_StartTime = 0;
        private double m_MonsterDeadTime = -1;
        private double m_UserDeadTime = -1;
        private TollgateData[] m_Tollgates = new TollgateData[c_MaxExpeditionNum];
    }
}
