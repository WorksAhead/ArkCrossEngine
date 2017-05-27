using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal class ExpeditionImageInfo
    {
        internal ExpeditionImageInfo()
        {
        }
        internal ExpeditionImageInfo(UserInfo user)
        {
            this.Guid = user.Guid;
            this.HeroId = user.HeroId;
            this.Nickname = user.Nickname;
            this.Level = user.Level;
            this.FightingScore = user.FightingScore;
            if (user.Skill.Skills.Count > 0)
            {
                Skills.Reset();
                int ct = user.Skill.Skills.Count;
                for (int i = 0; i < ct; i++)
                {
                    Skills.AddSkillData(user.Skill.Skills[i]);
                }
            }
            if (Equips.Armor.Length == user.Equip.Armor.Length)
            {
                Equips.Reset();
                int ct = Equips.Armor.Length;
                for (int i = 0; i < ct; i++)
                {
                    Equips.Armor[i] = user.Equip.Armor[i];
                }
            }
            if (Legacys.SevenArcs.Length == user.Legacy.SevenArcs.Length)
            {
                Legacys.Reset();
                int ct = Legacys.SevenArcs.Length;
                for (int i = 0; i < ct; i++)
                {
                    Legacys.SevenArcs[i] = user.Legacy.SevenArcs[i];
                }
            }
        }
        internal ulong Guid
        {
            get { return m_Guid; }
            set { m_Guid = value; }
        }
        internal int HeroId
        {
            get { return m_HeroId; }
            set { m_HeroId = value; }
        }
        internal string Nickname
        {
            get { return m_Nickname; }
            set { m_Nickname = value; }
        }
        internal int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        internal int FightingScore
        {
            get { return m_FightingScore; }
            set { m_FightingScore = value; }
        }
        internal EquipInfo Equips
        {
            get { return m_EquipInfo; }
            set { m_EquipInfo = value; }
        }
        internal SkillInfo Skills
        {
            get { return m_SkillInfo; }
            set { m_SkillInfo = value; }
        }
        internal LegacyInfo Legacys
        {
            get { return m_LegacyInfo; }
            set { m_LegacyInfo = value; }
        }
        private ulong m_Guid = 0;
        private int m_HeroId = 0;
        private string m_Nickname = "";
        private int m_Level = 1;
        private int m_FightingScore = 0;
        private EquipInfo m_EquipInfo = new EquipInfo();
        private SkillInfo m_SkillInfo = new SkillInfo();
        private LegacyInfo m_LegacyInfo = new LegacyInfo();
    }
    ///
    internal enum EnemyType : int
    {
        ET_Monster = 0,
        ET_Boss,
        ET_OnePlayer,
        ET_TwoPlayer,
    }
    internal class ExpeditionPlayerInfo
    {
        internal const int c_MaxExpeditionNum = 12;

        internal class AwardItemData
        {
            internal AwardItemData()
            {
                this.m_ItemId = 0;
                this.m_ItemNum = 0;
            }
            internal int ItemId
            {
                get { return m_ItemId; }
                set { m_ItemId = value; }
            }
            internal int ItemNum
            {
                get { return m_ItemNum; }
                set { m_ItemNum = value; }
            }
            private int m_ItemId;
            private int m_ItemNum;
        }
        internal class TollgateData
        {
            internal TollgateData()
            {
                this.m_Type = EnemyType.ET_Monster;
                this.EnemyAttrList.Clear();
                this.m_EnemyList.Clear();
                this.m_UserImageList.Clear();
                this.m_IsFinish = false;
                this.m_IsAcceptedAward = true;
            }
            internal void Reset()
            {
                this.m_Type = EnemyType.ET_Monster;
                m_EnemyAttrList.Clear();
                m_EnemyList.Clear();
                m_UserImageList.Clear();
                this.m_IsFinish = false;
                this.m_IsAcceptedAward = true;
            }
            internal EnemyType Type
            {
                get { return m_Type; }
                set { m_Type = value; }
            }
            internal List<int> EnemyList
            {
                get { return m_EnemyList; }
            }
            internal List<int> EnemyAttrList
            {
                get { return m_EnemyAttrList; }
            }
            internal List<ExpeditionImageInfo> UserImageList
            {
                get { return m_UserImageList; }
            }
            internal bool IsFinish
            {
                get { return m_IsFinish; }
                set { m_IsFinish = value; }
            }
            internal bool IsAcceptedAward
            {
                get { return m_IsAcceptedAward; }
                set { m_IsAcceptedAward = value; }
            }
            private EnemyType m_Type = EnemyType.ET_Monster;
            private List<int> m_EnemyList = new List<int>();
            private List<int> m_EnemyAttrList = new List<int>();
            private List<ExpeditionImageInfo> m_UserImageList = new List<ExpeditionImageInfo>();
            private bool m_IsFinish = false;
            private bool m_IsAcceptedAward = true;
        }
        ///
        internal ExpeditionPlayerInfo()
        {
            this.m_Hp = 0;
            this.m_Mp = 0;
            this.m_Rage = 0;
            this.m_Schedule = 0;
            this.m_CurResetCount = 0;
            this.m_CanReset = true;
            this.m_CurWeakMonsterCount = 0;
            this.m_CurBossCount = 0;
            this.m_CurOnePlayerCount = 0;
            for (int i = 0; i < m_Tollgates.Length; i++)
            {
                m_Tollgates[i] = new TollgateData();
            }
        }
        internal void ResetData()
        {
            this.m_Hp = 0;
            this.m_Mp = 0;
            this.m_Rage = 0;
            this.m_Schedule = 0;
            this.m_CurResetCount = 0;
            this.m_CanReset = true;
            this.m_CurWeakMonsterCount = 0;
            this.m_CurBossCount = 0;
            this.m_CurOnePlayerCount = 0;
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
        internal void ResetExpeditionTime()
        {
            if (TimeUtility.CurTimestamp - LastResetTimestamp >= c_ExpeditionResetIntervalTime && !m_CanReset)
            {
                CanReset = true;
            }
        }
        ///
        private int MatchMaxScore(int score, MonsterType type, out int match_score, out int match_attr)
        {
            match_attr = 0;
            match_score = 0;
            int min_monster_score = c_DefaultMinMonsterScore;
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
        ///
        private int MatchMinScore(int score, MonsterType type, out int match_score, out int match_attr)
        {
            match_attr = 0;
            match_score = 0;
            int max_monster_score = 0;
            int max_monster_id = 0;
            for (int index = 0; index < ExpeditionMonsterConfigProvider.Instance.GetDataCount(); index++)
            {
                ExpeditionMonsterConfig monster_data = ExpeditionMonsterConfigProvider.Instance.GetDataById(index) as ExpeditionMonsterConfig;
                if (null != monster_data && type == monster_data.m_Type)
                {
                    int cur_score = monster_data.m_FightingScore;
                    if (cur_score > max_monster_score && cur_score < score)
                    {
                        match_score = cur_score;
                        max_monster_score = cur_score;
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
                                max_monster_id = link_id;
                                match_attr = rd_attr;
                            }
                        }
                    }
                }
            }
            return max_monster_id;
        }
        ///
        private class MatchArg
        {
            internal MatchArg(int id, int attr)
            {
                this.Id = id;
                this.Attr = attr;
            }
            internal int Id { get; set; }
            internal int Attr { get; set; }
        }
        private void RefixMonsterData(MonsterType type, List<MatchArg> list)
        {
            if (null == list)
                return;
            if (MonsterType.MT_Normal == type)
            {
                foreach (MatchArg arg in list)
                {
                    if (null != arg && 0 == arg.Id && 0 == arg.Attr)
                    {
                        arg.Id = c_DefSmlMonsterId;
                        arg.Attr = c_DefSmlMonsterAttr;
                    }
                }
            }
            else if (MonsterType.MT_Boss == type)
            {
                foreach (MatchArg arg in list)
                {
                    if (null != arg && 0 == arg.Id && 0 == arg.Attr)
                    {
                        arg.Id = c_DefBigMonsterId;
                        arg.Attr = c_DefBigMonsterAttr;
                    }
                }
            }
        }
        ///
        private List<MatchArg> MatchMonster(int score, MonsterType type, bool weaken)
        {
            List<MatchArg> match_list = new List<MatchArg>();
            if (MonsterType.MT_Normal == type)
            {
                int max_first_attr = 0;
                int max_second_attr = 0;
                int max_third_attr = 0;
                int max_fourth_attr = 0;
                int out_max_match_score = 0;
                int max_first = MatchMaxScore(score, type, out out_max_match_score, out max_first_attr);
                int max_second = MatchMaxScore(out_max_match_score, type, out out_max_match_score, out max_second_attr);
                int max_third = MatchMaxScore(out_max_match_score, type, out out_max_match_score, out max_third_attr);
                int max_fourth = MatchMaxScore(out_max_match_score, type, out out_max_match_score, out max_fourth_attr);
                int min_first_attr = 0;
                int min_second_attr = 0;
                int min_third_attr = 0;
                int out_min_match_score = 0;
                int min_first = MatchMinScore(score, type, out out_min_match_score, out min_first_attr);
                int min_second = MatchMinScore(out_min_match_score, type, out out_min_match_score, out min_second_attr);
                int min_third = MatchMinScore(out_min_match_score, type, out out_min_match_score, out min_third_attr);
                if (0 == min_first)
                {
                    min_first = MatchMaxScore(out_max_match_score, type, out out_min_match_score, out min_first_attr);
                    min_second = MatchMaxScore(out_min_match_score, type, out out_min_match_score, out min_second_attr);
                    min_third = MatchMaxScore(out_min_match_score, type, out out_min_match_score, out min_third_attr);
                }
                if (0 == min_second)
                {
                    min_second = MatchMaxScore(out_max_match_score, type, out out_min_match_score, out min_second_attr);
                    min_third = MatchMaxScore(out_min_match_score, type, out out_min_match_score, out min_third_attr);
                }
                if (0 == min_third)
                {
                    min_third = MatchMaxScore(out_max_match_score, type, out out_min_match_score, out min_third_attr);
                }
                MatchArg min_third_obj = new MatchArg(min_third, min_third_attr);
                match_list.Add(min_third_obj);
                MatchArg min_second_obj = new MatchArg(min_second, min_second_attr);
                match_list.Add(min_second_obj);
                MatchArg min_first_obj = new MatchArg(min_first, min_first_attr);
                match_list.Add(min_first_obj);
                MatchArg max_first_obj = new MatchArg(max_first, max_first_attr);
                match_list.Add(max_first_obj);
                MatchArg max_second_obj = new MatchArg(max_second, max_second_attr);
                match_list.Add(max_second_obj);
                MatchArg max_third_obj = new MatchArg(max_third, max_third_attr);
                match_list.Add(max_third_obj);
                MatchArg max_fourth_obj = new MatchArg(max_fourth, max_fourth_attr);
                match_list.Add(max_fourth_obj);
                RefixMonsterData(type, match_list);
            }
            else if (MonsterType.MT_Boss == type)
            {
                if (weaken)
                {
                    int max_first_attr = 0;
                    int out_max_match_score = 0;
                    int max_first = MatchMaxScore(score, type, out out_max_match_score, out max_first_attr);
                    MatchArg max_first_obj = new MatchArg(max_first, max_first_attr);
                    match_list.Add(max_first_obj);
                }
                else
                {
                    int max_first_attr = 0;
                    int out_max_match_score = 0;
                    int max_first = MatchMaxScore(score, type, out out_max_match_score, out max_first_attr);
                    int min_first_attr = 0;
                    int out_min_match_score = 0;
                    int min_first = MatchMinScore(score, type, out out_min_match_score, out min_first_attr);
                    if (0 == min_first)
                    {
                        min_first = MatchMaxScore(out_max_match_score, type, out out_min_match_score, out min_first_attr);
                    }
                    MatchArg min_first_obj = new MatchArg(min_first, min_first_attr);
                    match_list.Add(min_first_obj);
                    MatchArg max_first_obj = new MatchArg(max_first, max_first_attr);
                    match_list.Add(max_first_obj);
                }
                RefixMonsterData(type, match_list);
            }
            return match_list.Count > 0 ? match_list : null;
        }
        internal GeneralOperationResult RequestExpeditionInfo(ulong guid, int request_num, bool is_reset, SortedDictionary<int, ExpeditionImageInfo> dictionary)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user || null == dictionary)
            {
                return GeneralOperationResult.LC_Failure_Unknown;
            }
            GeneralOperationResult result = GeneralOperationResult.LC_Succeed;
            if (user != null && null != user.Expedition && null != user.Expedition.Tollgates)
            {
                if (user.Expedition.CanReset || /*0 != user.Expedition.Schedule*/ 0 != request_num)
                {
                    if (user.Level >= ExpeditionPlayerInfo.UnlockLevel)
                    {
                        if (0 == request_num && is_reset)
                        {
                            ResetData();
                            user.Expedition.ResetScore = user.FightingScore - user.Skill.GetSkillAppendScore();
                            user.Expedition.CanReset = false;
                            user.Expedition.LastResetTimestamp = TimeUtility.CurTimestamp;
                        }
                        int user_score = user.Expedition.ResetScore;
                        int user_blur_value = (int)(ExpeditionPlayerInfo.c_BlurValue * user_score);
                        for (int i = 0; i < user.Expedition.Tollgates.Length; i++)
                        {
                            if (request_num != i)
                            {
                                continue;
                            }
                            TollgateData tollgate_info = user.Expedition.Tollgates[i];
                            if (null != tollgate_info)
                            {
                                int cur_tollgate_score = user_score;
                                ExpeditionTollgateConfig cur_tollgate_data = ExpeditionTollgateConfigProvider.Instance.GetDataById(i + 1) as ExpeditionTollgateConfig;
                                if (null != cur_tollgate_data)
                                {
                                    cur_tollgate_score = (int)(user_score * cur_tollgate_data.m_RelativeScore);
                                }
                                int cur_enemy_min_score = (int)(cur_tollgate_score * ExpeditionPlayerInfo.c_MonsterTollgateRelativeScore) - user_blur_value;
                                int cur_enemy_max_score = (int)(cur_tollgate_score * ExpeditionPlayerInfo.c_MonsterTollgateRelativeScore) + user_blur_value;
                                if (i >= 0 && i < 5)
                                {
                                    EnemyType cur_enemy_type = EnemyType.ET_Monster;
                                    if (i > 3)
                                    {
                                        cur_enemy_type = (EnemyType)CrossEngineHelper.Random.Next(0, 2);
                                    }
                                    MonsterType cur_monster_type = MonsterType.MT_Normal;
                                    int cur_match_monster_score = cur_tollgate_score;
                                    if (EnemyType.ET_Monster == cur_enemy_type)
                                    {
                                        cur_match_monster_score = (int)(cur_tollgate_score * ExpeditionPlayerInfo.c_MonsterTollgateRelativeScore);
                                        user.Expedition.CurWeakMonsterCount += 1;
                                        cur_monster_type = MonsterType.MT_Normal;
                                    }
                                    else if (EnemyType.ET_Boss == cur_enemy_type)
                                    {
                                        if (user.Expedition.CurBossCount + 1 > ExpeditionPlayerInfo.c_BossMax)
                                        {
                                            cur_match_monster_score = (int)(cur_tollgate_score * ExpeditionPlayerInfo.c_MonsterTollgateRelativeScore);
                                            cur_enemy_type = EnemyType.ET_Monster;
                                            cur_monster_type = MonsterType.MT_Normal;
                                            user.Expedition.CurWeakMonsterCount += 1;
                                        }
                                        else
                                        {
                                            cur_monster_type = MonsterType.MT_Boss;
                                            user.Expedition.CurBossCount += 1;
                                        }
                                    }
                                    tollgate_info.Type = cur_enemy_type;
                                    List<MatchArg> monster_match_list = MatchMonster(cur_match_monster_score, cur_monster_type, true);
                                    if (null != monster_match_list)
                                    {
                                        int mmlct = monster_match_list.Count;
                                        for (int assit_index = 0; assit_index < mmlct; assit_index++)
                                        {
                                            tollgate_info.EnemyList.Add(monster_match_list[assit_index].Id);
                                            tollgate_info.EnemyAttrList.Add(monster_match_list[assit_index].Attr);
                                        }
                                    }
                                    if (tollgate_info.EnemyList.Count <= 0)
                                    {
                                        result = GeneralOperationResult.LC_Failure_Unknown;
                                        break;
                                    }
                                }
                                else if (i >= 5 && i < 11)
                                {
                                    EnemyType cur_enemy_type = (EnemyType)CrossEngineHelper.Random.Next(0, 3);
                                    cur_enemy_type = SelectEnemyType(guid, cur_enemy_type);
                                    List<ExpeditionImageInfo> match_user_image_list = new List<ExpeditionImageInfo>();
                                    if (EnemyType.ET_OnePlayer == cur_enemy_type)
                                    {
                                        if (dictionary.Count > 1)
                                        {
                                            cur_enemy_min_score = cur_tollgate_score - user_blur_value;
                                            cur_enemy_max_score = cur_tollgate_score + user_blur_value;
                                            for (int index = cur_enemy_min_score; index <= cur_enemy_max_score; index++)
                                            {
                                                if (null != dictionary)
                                                {
                                                    int same_num = 20;
                                                    for (int incr = 0; incr < same_num; incr++)
                                                    {
                                                        ExpeditionImageInfo match_user_image = null;
                                                        if (dictionary.TryGetValue(index * 10000 + incr, out match_user_image))
                                                        {
                                                            match_user_image_list.Add(match_user_image);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            /// only playerself
                                            foreach (ExpeditionImageInfo Image in dictionary.Values)
                                            {
                                                match_user_image_list.Add(Image);
                                                break;
                                            }
                                        }
                                        ///
                                        int match_num = match_user_image_list.Count;
                                        if (match_num <= 0)
                                        {
                                            foreach (ExpeditionImageInfo Image in dictionary.Values)
                                            {
                                                bool selected = CrossEngineHelper.Random.Next(0, 101) > 50 ? true : false;
                                                if (selected)
                                                {
                                                    match_user_image_list.Add(Image);
                                                    break;
                                                }
                                            }
                                        }
                                        int total_match_num = match_user_image_list.Count;
                                        if (total_match_num <= 0)
                                        {
                                            foreach (ExpeditionImageInfo Image in dictionary.Values)
                                            {
                                                match_user_image_list.Add(Image);
                                                break;
                                            }
                                        }
                                        ///
                                        if (match_user_image_list.Count > 0)
                                        {
                                            tollgate_info.Type = cur_enemy_type;
                                            int rnd_index = CrossEngineHelper.Random.Next(0, match_num);
                                            tollgate_info.UserImageList.Add(match_user_image_list[rnd_index]);
                                        }
                                        else
                                        {
                                            result = GeneralOperationResult.LC_Failure_Unknown;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        MonsterType cur_monster_type = MonsterType.MT_Normal;
                                        int cur_match_monster_score = cur_tollgate_score;
                                        if (EnemyType.ET_Boss == cur_enemy_type)
                                        {
                                            cur_monster_type = MonsterType.MT_Boss;
                                            tollgate_info.Type = cur_enemy_type;
                                        }
                                        else if (EnemyType.ET_Monster == cur_enemy_type)
                                        {
                                            cur_monster_type = MonsterType.MT_Normal;
                                            cur_match_monster_score = (int)(cur_tollgate_score * ExpeditionPlayerInfo.c_MonsterTollgateRelativeScore);
                                        }
                                        List<MatchArg> monster_match_list = MatchMonster(cur_match_monster_score, cur_monster_type, false);
                                        if (null != monster_match_list)
                                        {
                                            int mmlct = monster_match_list.Count;
                                            for (int assit_index = 0; assit_index < mmlct; assit_index++)
                                            {
                                                tollgate_info.EnemyList.Add(monster_match_list[assit_index].Id);
                                                tollgate_info.EnemyAttrList.Add(monster_match_list[assit_index].Attr);
                                            }
                                        }
                                        if (tollgate_info.EnemyList.Count <= 0)
                                        {
                                            result = GeneralOperationResult.LC_Failure_Unknown;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    List<ExpeditionImageInfo> match_user_image_list = new List<ExpeditionImageInfo>();
                                    if (dictionary.Count > 1)
                                    {
                                        cur_enemy_min_score = (int)(cur_tollgate_score * ExpeditionPlayerInfo.c_TwoPlayerTollgateRelativeScore) - user_blur_value;
                                        cur_enemy_max_score = (int)(cur_tollgate_score * ExpeditionPlayerInfo.c_TwoPlayerTollgateRelativeScore) + user_blur_value;
                                        for (int index = cur_enemy_min_score; index <= cur_enemy_max_score; index++)
                                        {
                                            if (null != dictionary)
                                            {
                                                int same_num = 20;
                                                for (int incr = 0; incr < same_num; incr++)
                                                {
                                                    ExpeditionImageInfo match_user_image = null;
                                                    if (dictionary.TryGetValue(index * 10000 + incr, out match_user_image))
                                                    {
                                                        match_user_image_list.Add(match_user_image);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        /// only playerself
                                        foreach (ExpeditionImageInfo Image in dictionary.Values)
                                        {
                                            match_user_image_list.Add(Image);
                                            break;
                                        }
                                    }
                                    ///
                                    int match_num = match_user_image_list.Count;
                                    if (match_num <= 0)
                                    {
                                        foreach (ExpeditionImageInfo Image in dictionary.Values)
                                        {
                                            bool selected = CrossEngineHelper.Random.Next(0, 101) > 50 ? true : false;
                                            if (selected)
                                            {
                                                match_user_image_list.Add(Image);
                                                break;
                                            }
                                        }
                                    }
                                    int total_match_num = match_user_image_list.Count;
                                    if (total_match_num <= 0)
                                    {
                                        foreach (ExpeditionImageInfo Image in dictionary.Values)
                                        {
                                            match_user_image_list.Add(Image);
                                            break;
                                        }
                                    }
                                    ///
                                    if (match_user_image_list.Count > 0)
                                    {
                                        tollgate_info.Type = EnemyType.ET_TwoPlayer;
                                        if (1 == match_num)
                                        {
                                            int fixed_num = 0;
                                            tollgate_info.UserImageList.Add(match_user_image_list[fixed_num]);
                                            tollgate_info.UserImageList.Add(match_user_image_list[fixed_num]);
                                        }
                                        else
                                        {
                                            int f_rnd_num = CrossEngineHelper.Random.Next(0, match_num);
                                            int s_rnd_num = CrossEngineHelper.Random.Next(0, match_num);
                                            tollgate_info.UserImageList.Add(match_user_image_list[f_rnd_num]);
                                            tollgate_info.UserImageList.Add(match_user_image_list[s_rnd_num]);
                                        }
                                    }
                                    else
                                    {
                                        result = GeneralOperationResult.LC_Failure_Unknown;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_LevelError;
                    }
                }
                else
                {
                    result = GeneralOperationResult.LC_Failure_Time;
                }
            }
            else
            {
                result = GeneralOperationResult.LC_Failure_LevelError;
            }
            return result;
        }
        ///
        private EnemyType SelectEnemyType(ulong guid, EnemyType random_enemy_type)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user)
            {
                return random_enemy_type;
            }
            ExpeditionPlayerInfo expedition = user.Expedition;
            if (null == expedition)
            {
                return random_enemy_type;
            }
            if (EnemyType.ET_Monster == random_enemy_type)
            {
                if (expedition.CurWeakMonsterCount + 1 <= ExpeditionPlayerInfo.c_WeakMonsterMax)
                {
                    expedition.CurWeakMonsterCount += 1;
                    return EnemyType.ET_Monster;
                }
                if (expedition.CurBossCount + 1 <= ExpeditionPlayerInfo.c_BossMax)
                {
                    expedition.CurBossCount += 1;
                    return EnemyType.ET_Boss;
                }
                if (expedition.CurOnePlayerCount + 1 <= ExpeditionPlayerInfo.c_OnePlayerMax)
                {
                    expedition.CurOnePlayerCount += 1;
                    return EnemyType.ET_OnePlayer;
                }
            }
            else if (EnemyType.ET_Boss == random_enemy_type)
            {
                if (expedition.CurBossCount + 1 <= ExpeditionPlayerInfo.c_BossMax)
                {
                    expedition.CurBossCount += 1;
                    return EnemyType.ET_Boss;
                }
                if (expedition.CurWeakMonsterCount + 1 <= ExpeditionPlayerInfo.c_WeakMonsterMax)
                {
                    expedition.CurWeakMonsterCount += 1;
                    return EnemyType.ET_Monster;
                }
                if (expedition.CurOnePlayerCount + 1 <= ExpeditionPlayerInfo.c_OnePlayerMax)
                {
                    expedition.CurOnePlayerCount += 1;
                    return EnemyType.ET_OnePlayer;
                }
            }
            else if (EnemyType.ET_OnePlayer == random_enemy_type)
            {
                if (expedition.CurOnePlayerCount + 1 <= ExpeditionPlayerInfo.c_OnePlayerMax)
                {
                    expedition.CurOnePlayerCount += 1;
                    return EnemyType.ET_OnePlayer;
                }
                if (expedition.CurBossCount + 1 <= ExpeditionPlayerInfo.c_BossMax)
                {
                    expedition.CurBossCount += 1;
                    return EnemyType.ET_Boss;
                }
                if (expedition.CurWeakMonsterCount + 1 <= ExpeditionPlayerInfo.c_WeakMonsterMax)
                {
                    expedition.CurWeakMonsterCount += 1;
                    return EnemyType.ET_Monster;
                }
            }
            return random_enemy_type;
        }
        ///
        internal void SyncExpeditionInfo(ulong guid, int hp, int mp, int rage, int request_num, GeneralOperationResult result)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && null != user.Expedition && null != user.Expedition.Tollgates)
            {
                JsonMessageWithGuid resetMsg = new JsonMessageWithGuid(JsonMessageID.ExpeditionResetResult);
                resetMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult protoData = new ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult();
                if (GeneralOperationResult.LC_Succeed == result)
                {
                    ExpeditionPlayerInfo expedition = user.Expedition;
                    expedition.Hp = hp;
                    expedition.Mp = mp;
                    expedition.Rage = rage;
                    ///
                    protoData.m_Hp = expedition.Hp;
                    protoData.m_Mp = expedition.Mp;
                    protoData.m_Rage = expedition.Rage;
                    protoData.m_Schedule = expedition.Schedule;
                    protoData.m_LastResetTimestamp = (int)expedition.LastResetTimestamp;
                    protoData.m_CanReset = expedition.CanReset;
                    protoData.m_IsUnlock = expedition.IsUnlock;
                    ///
                    int index = request_num > 0 ? request_num : expedition.Schedule;
                    if (index < c_MaxExpeditionNum)
                    {
                        ExpeditionPlayerInfo.TollgateData data = user.Expedition.Tollgates[index];
                        if (null != data)
                        {
                            protoData.Tollgates = new ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult.TollgateDataForMsg();
                            protoData.Tollgates.Type = (int)data.Type;
                            protoData.Tollgates.IsFinish = data.IsFinish;
                            int tollgates_ct = user.Expedition.Tollgates.Length;
                            for (int i = 0; i < tollgates_ct; i++)
                            {
                                protoData.Tollgates.IsAcceptedAward.Add(user.Expedition.Tollgates[i].IsAcceptedAward);
                            }
                            if (EnemyType.ET_Boss == data.Type || EnemyType.ET_Monster == data.Type)
                            {
                                if (data.EnemyList.Count > 0)
                                {
                                    for (int assit_index = 0; assit_index < data.EnemyList.Count; assit_index++)
                                    {
                                        protoData.Tollgates.EnemyArray.Add(data.EnemyList[assit_index]);
                                        protoData.Tollgates.EnemyAttrArray.Add(data.EnemyAttrList[assit_index]);
                                    }
                                }
                            }
                            else
                            {
                                if (data.UserImageList.Count > 0)
                                {
                                    for (int assit_index = 0; assit_index < data.UserImageList.Count; assit_index++)
                                    {
                                        ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult.ImageDataMsg image_data_msg = new ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult.ImageDataMsg();
                                        image_data_msg.Guid = data.UserImageList[assit_index].Guid;
                                        image_data_msg.HeroId = data.UserImageList[assit_index].HeroId;
                                        image_data_msg.Level = data.UserImageList[assit_index].Level;
                                        image_data_msg.Nickname = data.UserImageList[assit_index].Nickname;
                                        image_data_msg.FightingScore = (int)(data.UserImageList[assit_index].FightingScore / 10000.0f);
                                        /// equips
                                        int equips_num = data.UserImageList[assit_index].Equips.Armor.Length;
                                        if (equips_num > 0)
                                        {
                                            for (int i = 0; i < equips_num; i++)
                                            {
                                                ItemInfo item_info = data.UserImageList[assit_index].Equips.Armor[i];
                                                if (null != item_info)
                                                {
                                                    ArkCrossEngineMessage.ItemDataMsg equip_data = new ArkCrossEngineMessage.ItemDataMsg();
                                                    equip_data.ItemId = item_info.ItemId;
                                                    equip_data.Level = item_info.Level;
                                                    equip_data.Num = item_info.ItemNum;
                                                    equip_data.AppendProperty = item_info.AppendProperty;
                                                    image_data_msg.EquipInfo.Add(equip_data);
                                                }
                                            }
                                        }
                                        /// skills
                                        int skills_num = data.UserImageList[assit_index].Skills.Skills.Count;
                                        if (skills_num > 0)
                                        {
                                            for (int i = 0; i < skills_num; i++)
                                            {
                                                SkillDataInfo skill_info = data.UserImageList[assit_index].Skills.Skills[i];
                                                if (null != skill_info)
                                                {
                                                    ArkCrossEngineMessage.SkillDataInfo skill_data = new ArkCrossEngineMessage.SkillDataInfo();
                                                    skill_data.ID = skill_info.ID;
                                                    skill_data.Level = skill_info.Level;
                                                    skill_data.Postions = (int)skill_info.Postions.Presets[0];
                                                    image_data_msg.SkillInfo.Add(skill_data);
                                                }
                                            }
                                        }
                                        /// legacys
                                        int legacys_num = data.UserImageList[assit_index].Legacys.SevenArcs.Length;
                                        if (legacys_num > 0)
                                        {
                                            for (int i = 0; i < legacys_num; i++)
                                            {
                                                ItemInfo item_info = data.UserImageList[assit_index].Legacys.SevenArcs[i];
                                                if (null != item_info)
                                                {
                                                    ArkCrossEngineMessage.LegacyDataMsg legacy_data = new ArkCrossEngineMessage.LegacyDataMsg();
                                                    legacy_data.ItemId = item_info.ItemId;
                                                    legacy_data.Level = item_info.Level;
                                                    legacy_data.IsUnlock = item_info.IsUnlock;
                                                    legacy_data.AppendProperty = item_info.AppendProperty;
                                                    image_data_msg.LegacyInfo.Add(legacy_data);
                                                }
                                            }
                                        }
                                        protoData.Tollgates.UserImageArray.Add(image_data_msg);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        protoData.Tollgates = new ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult.TollgateDataForMsg();
                        protoData.Tollgates.Type = 0;
                        protoData.Tollgates.IsFinish = true;
                        int toll_ct = user.Expedition.Tollgates.Length;
                        for (int i = 0; i < toll_ct; i++)
                        {
                            protoData.Tollgates.IsAcceptedAward.Add(user.Expedition.Tollgates[i].IsAcceptedAward);
                        }
                    }
                }
                else if (GeneralOperationResult.LC_Failure_LevelError != result
              && GeneralOperationResult.LC_Failure_Time != result)
                {
                    user.Expedition.ResetData();
                }
                protoData.m_Result = (int)result;
                resetMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, resetMsg);
            }
        }
        ///
        internal int Hp
        {
            get { return m_Hp; }
            set { m_Hp = value; }
        }
        internal int Mp
        {
            get { return m_Mp; }
            set { m_Mp = value; }
        }
        internal int Rage
        {
            get { return m_Rage; }
            set { m_Rage = value; }
        }
        internal TollgateData[] Tollgates
        {
            get { return m_Tollgates; }
        }
        internal int Schedule
        {
            get { return m_Schedule; }
            set { m_Schedule = value; }
        }
        internal double LastResetTimestamp
        {
            get { return m_LastResetTimestamp; }
            set { m_LastResetTimestamp = value; }
        }
        internal int CurResetCount
        {
            get { return m_CurResetCount; }
            set { m_CurResetCount = value; }
        }
        internal bool CanReset
        {
            get { return m_CanReset; }
            set { m_CanReset = value; }
        }
        internal int CurWeakMonsterCount
        {
            get { return m_CurWeakMonsterCount; }
            set { m_CurWeakMonsterCount = value; }
        }
        internal int CurBossCount
        {
            get { return m_CurBossCount; }
            set { m_CurBossCount = value; }
        }
        internal int CurOnePlayerCount
        {
            get { return m_CurOnePlayerCount; }
            set { m_CurOnePlayerCount = value; }
        }
        internal bool IsUnlock
        {
            get { return m_IsUnlock; }
            set { m_IsUnlock = value; }
        }
        internal int ResetScore
        {
            get { return m_ResetScore; }
            set { m_ResetScore = value; }
        }
        internal const double c_ExpeditionResetIntervalTime = 10800;
        internal const int c_ResetMax = 5;
        internal const float c_BlurValue = 0.3f;
        internal const float c_MonsterTollgateRelativeScore = 1.0f;
        internal const float c_TwoPlayerTollgateRelativeScore = 0.9f;
        internal const int c_WeakMonsterKind = 4;
        internal const int c_OnePlayerKind = 1;
        internal const int c_BossKind = 1;
        internal const int c_WeakMonsterMax = 6;
        internal const int c_BossMax = 2;
        internal const int c_OnePlayerMax = 3;
        internal const int c_DefaultMinMonsterScore = 1000000;
        internal static int UnlockLevel = 18;
        private const int c_DefSmlMonsterId = 80001;
        private const int c_DefSmlMonsterAttr = 21;
        private const int c_DefBigMonsterId = 83001;
        private const int c_DefBigMonsterAttr = 3021;
        private bool m_IsUnlock = false;
        private int m_ResetScore = 0;
        private int m_Hp;
        private int m_Mp;
        private int m_Rage;
        private int m_Schedule;
        private int m_CurWeakMonsterCount = 0;
        private int m_CurBossCount = 0;
        private int m_CurOnePlayerCount = 0;
        private int m_CurResetCount = 0;
        private double m_LastResetTimestamp = 0;
        private bool m_CanReset = true;
        private TollgateData[] m_Tollgates = new TollgateData[c_MaxExpeditionNum];
    }
}
