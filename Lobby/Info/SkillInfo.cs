using System;
using System.Collections.Generic;
using CSharpCenterClient;
using Lobby_RoomServer;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    public enum SlotPosition : int
    {
        SP_None = 0,
        SP_A,
        SP_B,
        SP_C,
        SP_D,
    }

    public class PresetInfo
    {
        public PresetInfo()
        {
            for (int i = 0; i < PresetNum; i++)
            {
                Presets[i] = SlotPosition.SP_None;
            }
        }
        public void Reset()
        {
            for (int i = 0; i < PresetNum; i++)
            {
                Presets[i] = SlotPosition.SP_None;
            }
        }
        public const int PresetNum = 4;
        public SlotPosition[] Presets = new SlotPosition[PresetNum];
    }

    public class SkillDataInfo
    {
        public SkillDataInfo()
        {
            this.ID = -1;
            this.Level = 0;
            this.Postions = new PresetInfo();
        }
        public SkillDataInfo(int id, int level = 0)
        {
            this.ID = id;
            this.Level = level;
            this.Postions = new PresetInfo();
        }
        public int ID { get; set; }
        public int Level { get; set; }
        public PresetInfo Postions { get; set; }
    }

    internal class SkillInfo
    {
        internal object Lock
        {
            get { return m_Lock; }
        }
        internal SkillInfo()
        {
        }
        internal void AddSkillData(SkillDataInfo info)
        {
            lock (m_Lock)
            {
                if (null != info)
                {
                    m_Skills.Add(info);
                }
            }
        }
        internal void DelSkillDataByInfo(SkillDataInfo info)
        {
            lock (m_Lock)
            {
                if (null != info)
                {
                    m_Skills.Remove(info);
                }
            }
        }
        internal void DelSkillDataByID(int id)
        {
            SkillDataInfo del_info = GetSkillDataByID(id);
            if (null != del_info)
            {
                DelSkillDataByInfo(del_info);
            }
        }
        internal SkillDataInfo GetSkillDataByID(int id)
        {
            return m_Skills.Find(delegate (SkillDataInfo p) { return p.ID == id; });
        }
        private void EraseSlotPosition(int preset_index, SlotPosition sp)
        {
            for (int i = 0; i < m_Skills.Count; i++)
            {
                if (sp == m_Skills[i].Postions.Presets[preset_index])
                {
                    m_Skills[i].Postions.Presets[preset_index] = SlotPosition.SP_None;
                }
            }
        }
        internal void MountSkill(int preset_index, int skill_id, SlotPosition sp)
        {
            EraseSlotPosition(preset_index, sp);
            for (int i = 0; i < m_Skills.Count; i++)
            {
                if (skill_id == m_Skills[i].ID)
                {
                    m_Skills[i].Postions.Presets[preset_index] = sp;
                    break;
                }
            }
        }
        internal void UnmountSkill(int preset_index, SlotPosition sp)
        {
            EraseSlotPosition(preset_index, sp);
        }
        internal void UpgradeSkill(int skill_id, int next_level_skill_id)
        {
            for (int i = 0; i < m_Skills.Count; i++)
            {
                if (skill_id == m_Skills[i].ID)
                {
                    m_Skills[i].ID = next_level_skill_id;
                    break;
                }
            }
        }
        internal void IntensifySkill(int skill_id)
        {
            for (int i = 0; i < m_Skills.Count; i++)
            {
                if (skill_id == m_Skills[i].ID)
                {
                    m_Skills[i].Level += 1;
                    break;
                }
            }
        }
        internal void UnlockSkill(int skill_id)
        {
            for (int i = 0; i < m_Skills.Count; i++)
            {
                if (skill_id == m_Skills[i].ID)
                {
                    m_Skills[i].Level = 1;
                    break;
                }
            }
        }
        internal void SwapSkill(int preset_index, int skill_id, SlotPosition source_pos, SlotPosition target_pos)
        {
            for (int i = 0; i < m_Skills.Count; i++)
            {
                if (target_pos == m_Skills[i].Postions.Presets[preset_index])
                {
                    m_Skills[i].Postions.Presets[preset_index] = source_pos;
                    break;
                }
            }
            for (int i = 0; i < m_Skills.Count; i++)
            {
                if (skill_id == m_Skills[i].ID)
                {
                    m_Skills[i].Postions.Presets[preset_index] = target_pos;
                    break;
                }
            }
        }
        internal int GetSkillAppendScore()
        {
            int skill_append_score = 0;
            if (null != m_Skills && m_Skills.Count > 0)
            {
                foreach (SkillDataInfo skill in m_Skills)
                {
                    if ((int)skill.Postions.Presets[0] > 0)
                    {
                        SkillLogicData skill_data =
                          SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skill.ID) as SkillLogicData;
                        if (null != skill_data)
                            skill_append_score += skill_data.ShowScore;
                    }
                }
            }
            return skill_append_score;
        }
        internal void Reset()
        {
            m_Skills.Clear();
        }
        internal List<SkillDataInfo> Skills
        {
            get { return m_Skills; }
            set { m_Skills = value; }
        }
        internal int CurPresetIndex
        {
            get { return m_CurPresetIndex; }
            set { m_CurPresetIndex = value; }
        }

        private int m_CurPresetIndex = 0;
        private object m_Lock = new object();
        private List<SkillDataInfo> m_Skills = new List<SkillDataInfo>();
    }
}

