using System.Collections.Generic;
using ArkCrossEngine;

namespace GfxModule.Impact
{
    public sealed class GfxImpactSoundManager
    {
        public void Init()
        {
            HitSoundConfig config = HitSoundConfigProvider.Instance.GetData();
            if (null != config)
            {
                m_PeriodTime = config.PeriodTime;
                m_MaxPeriodCount = config.MaxPeriodCount;
            }
        }
        public void TryPlaySound(GameObject target, string sound)
        {
            if (CanPlaySoundNow())
            {
                if (null != target)
                {
                    ImpactUtility.PlaySound(target, sound);
                    RecordSoundPlay();
                }
                else
                {
                }
            }
        }
        public void TryPlaySound(int target, int impactId)
        {
            GameObject targetObj = LogicSystem.GetGameObject(target);
            if (null != targetObj)
            {
                SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(targetObj);
                if (null != shareInfo)
                {
                    ImpactLogicData config = (ImpactLogicData)SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_IMPACT, impactId);
                    if (null != config)
                    {
                        int soundCount = shareInfo.HitSounds.Count;
                        if (CrossEngineHelper.Random.NextFloat() < config.HitSoundProb && soundCount > 0)
                        {
                            string sound = shareInfo.HitSounds[CrossEngineHelper.Random.Next(soundCount)];
                            TryPlaySound(targetObj, sound);
                        }
                    }
                }
            }
        }
        public void UpdateSoundInfos()
        {
            List<GfxImpactSoundInfo> deletes = new List<GfxImpactSoundInfo>();
            for (int i = 0; i < m_SoundInfos.Count; ++i)
            {
                if (Time.time - m_SoundInfos[i].StartTime > m_PeriodTime)
                {
                    deletes.Add(m_SoundInfos[i]);
                }
            }
            for (int i = 0; i < deletes.Count; ++i)
            {
                m_SoundInfos.Remove(deletes[i]);
            }
        }
        public static GfxImpactSoundManager Instacne
        {
            get { return s_Instance; }
        }
        private bool CanPlaySoundNow()
        {
            return m_SoundInfos.Count < m_MaxPeriodCount;
        }
        private void RecordSoundPlay()
        {
            GfxImpactSoundInfo soundInfo = new GfxImpactSoundInfo();
            soundInfo.StartTime = Time.time;
            m_SoundInfos.Add(soundInfo);
        }
        private List<GfxImpactSoundInfo> m_SoundInfos = new List<GfxImpactSoundInfo>();
        private static GfxImpactSoundManager s_Instance = new GfxImpactSoundManager();
        private float m_PeriodTime = 8.0f;
        private int m_MaxPeriodCount = 8;
    }

    public class GfxImpactSoundInfo
    {
        public float StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }
        public float NeedDelete
        {
            get { return m_NeedDelete; }
            set { m_NeedDelete = value; }
        }
        private float m_StartTime;
        private float m_NeedDelete;
    }

}
