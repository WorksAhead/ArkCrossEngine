using System.Collections.Generic;
using ArkCrossEngine;

namespace GfxModule.Impact
{
    public class ImpactLogicInfo : IPoolAllocatedObject<ImpactLogicInfo>
    {
        private ImpactLogicInfo m_DowncastObj = null;
        private ObjectPool<ImpactLogicInfo> m_Pool = null;

        public void Recycle()
        {
            Reset();
            if (null != m_Pool)
            {
                m_Pool.Recycle(this);
            }
        }

        public void InitPool(ObjectPool<ImpactLogicInfo> pool)
        {
            m_DowncastObj = this as ImpactLogicInfo;
            m_Pool = pool;
        }

        public ImpactLogicInfo Downcast()
        {
            return m_DowncastObj;
        }

        public void Reset()
        {
            m_Sender = null;
            m_Target = null;
            m_LogicId = 0;
            m_ImpactId = 0;
            m_ConfigData = null;
            m_ConfigCacheData = null;
            m_ActionType = (int)Animation_Type.AT_Hurt0;
            m_StartTime = 0.0f;
            m_ElapsedTime = 0.0f;
            m_ElapsedTimeForEffect = 0.0f;   // 不收动作缩放的影响。
            m_Duration = 0.0f;
            m_IsActive = false;
            // Adjust info
            m_AdjustPoint = Vector3.zero;
            m_AdjustAppend = 0.0f;
            m_AdjustDegreeXZ = 0.0f;
            m_AdJustDegreeY = 0.0f;
            m_NormalEndPoint = Vector3.zero;
            m_OrignalPos = Vector3.zero;
            m_NormalPos = Vector3.zero;
            m_Velocity = Vector3.zero;
            m_Accelerate = Vector3.zero;
            m_MovingDelay = 0.0f;
            m_MovingTime = 0.0f;
            m_ImpactSrcPos = Vector3.zero;
            m_ImpactSrcDir = 0.0f;
            m_CustomDatas.Clear();
            // 分段动作，定帧，位移信息
            m_AnimationInfo = null;
            m_LockFrameInfo = null;
            m_MovementInfo = null;
            m_MoveDir = Quaternion.identity;
            // 特效信息
            for (int i = 0; i < m_EffectList.Count; ++i)
            {
                m_EffectList[i].Recycle();
            }
            m_EffectList.Clear();

            m_EffectsDelWithImpact.Clear();
        }

        public int LogicId
        {
            get { return m_LogicId; }
            set { m_LogicId = value; }
        }
        public float StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }
        public float Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }
        public float ElapsedTime
        {
            get { return m_ElapsedTime; }
            set { m_ElapsedTime = value; }
        }
        public float ElapsedTimeForEffect
        {
            get { return m_ElapsedTimeForEffect; }
            set { m_ElapsedTimeForEffect = value; }
        }
        public GameObject Sender
        {
            get { return m_Sender; }
            set { m_Sender = value; }
        }
        public GameObject Target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }
        public int ImpactId
        {
            get { return m_ImpactId; }
            set { m_ImpactId = value; }
        }
        public bool IsActive
        {
            get { return m_IsActive; }
            set { m_IsActive = value; }
        }
        public int ActionType
        {
            get { return m_ActionType; }
            set { m_ActionType = value; }
        }
        public ImpactLogicData ConfigData
        {
            get { return m_ConfigData; }
            set { m_ConfigData = value; }
        }

        public ImpactCacheData ConfigCacheData
        {
            get { return m_ConfigCacheData; }
            set { m_ConfigCacheData = value; }
        }


        public Vector3 ImpactSrcPos
        {
            get { return m_ImpactSrcPos; }
            set { m_ImpactSrcPos = value; }
        }
        public float ImpactSrcDir
        {
            get { return m_ImpactSrcDir; }
            set { m_ImpactSrcDir = value; }
        }
        public Vector3 Velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; }
        }
        public Vector3 AdjustPoint
        {
            get { return m_AdjustPoint; }
            set { m_AdjustPoint = value; }
        }
        public float AdjustAppend
        {
            get { return m_AdjustAppend; }
            set { m_AdjustAppend = value; }
        }
        public float AdjustDegreeXZ
        {
            get { return m_AdjustDegreeXZ; }
            set { m_AdjustDegreeXZ = value; }
        }
        public float AdjustDegreeY
        {
            get { return m_AdJustDegreeY; }
            set { m_AdJustDegreeY = value; }
        }
        public Vector3 NormalEndPoint
        {
            get { return m_NormalEndPoint; }
            set { m_NormalEndPoint = value; }
        }
        public Vector3 OrignalPos
        {
            get { return m_OrignalPos; }
            set { m_OrignalPos = value; }
        }
        public Vector3 NormalPos
        {
            get { return m_NormalPos; }
            set { m_NormalPos = value; }
        }
        public Vector3 Accelerate
        {
            get { return m_Accelerate; }
            set { m_Accelerate = value; }
        }
        public TypedDataCollection CustomDatas
        {
            get { return m_CustomDatas; }
            set { m_CustomDatas = value; }
        }
        public CurveInfo AnimationInfo
        {
            get { return m_AnimationInfo; }
            set { m_AnimationInfo = value; }
        }
        public CurveInfo LockFrameInfo
        {
            get { return m_LockFrameInfo; }
            set { m_LockFrameInfo = value; }
        }
        public CurveMoveInfo MovementInfo
        {
            get { return m_MovementInfo; }
            set { m_MovementInfo = value; }
        }
        public Quaternion MoveDir
        {
            get { return m_MoveDir; }
            set { m_MoveDir = value; }
        }
        public List<EffectInfo> EffectList
        {
            get { return m_EffectList; }
            set { m_EffectList = value; }
        }
        public List<GameObject> EffectsDelWithImpact
        {
            get { return m_EffectsDelWithImpact; }
        }


        public void AddEffectDataByinfo(int id, EffectInfo efinfo)
        {
            EffectLogicData effectData = (EffectLogicData)SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_EFFECT, id);
            if (null != effectData)
            {
                EffectInfo effectInfo = efinfo;
                effectInfo.IsActive = false;
                effectInfo.Path = effectData.EffectPath;
                effectInfo.PlayTime = effectData.PlayTime;
                effectInfo.RelativePoint = ImpactUtility.ConvertVector3D(effectData.RelativePos);
                effectInfo.RotateWithTarget = effectData.RotateWithTarget;
                effectInfo.RelativeRotation = ImpactUtility.ConvertVector3D(effectData.RelativeRotation);
                effectInfo.MountPoint = effectData.MountPoint;
                effectInfo.DelayTime = effectData.EffectDelay;
                effectInfo.DelWithImpact = effectData.DelWithImpact;
                m_EffectList.Add(effectInfo);
            }
            else
            {
                efinfo.Recycle();
            }
        }



        public void AddEffectData(int id)
        {
            EffectLogicData effectData = (EffectLogicData)SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_EFFECT, id);
            if (null != effectData)
            {
                EffectInfo effectInfo = new EffectInfo();
                effectInfo.IsActive = false;
                effectInfo.Path = effectData.EffectPath;
                effectInfo.PlayTime = effectData.PlayTime;
                effectInfo.RelativePoint = ImpactUtility.ConvertVector3D(effectData.RelativePos);
                effectInfo.RotateWithTarget = effectData.RotateWithTarget;
                effectInfo.RelativeRotation = ImpactUtility.ConvertVector3D(effectData.RelativeRotation);
                effectInfo.MountPoint = effectData.MountPoint;
                effectInfo.DelayTime = effectData.EffectDelay;
                effectInfo.DelWithImpact = effectData.DelWithImpact;
                m_EffectList.Add(effectInfo);
            }
        }
        protected GameObject m_Sender;
        protected GameObject m_Target;
        protected int m_LogicId = 0;
        protected int m_ImpactId = 0;
        protected ImpactLogicData m_ConfigData;
        protected ImpactCacheData m_ConfigCacheData;
        protected int m_ActionType = (int)Animation_Type.AT_Hurt0;
        protected float m_StartTime = 0.0f;
        protected float m_ElapsedTime = 0.0f;
        protected float m_ElapsedTimeForEffect = 0.0f;   // 不收动作缩放的影响。
        protected float m_Duration = 0.0f;
        protected bool m_IsActive = false;
        // Adjust info
        protected Vector3 m_AdjustPoint = new Vector3();
        protected float m_AdjustAppend;
        protected float m_AdjustDegreeXZ = 0;
        protected float m_AdJustDegreeY = 0;
        protected Vector3 m_NormalEndPoint;
        protected Vector3 m_OrignalPos;
        protected Vector3 m_NormalPos;
        protected Vector3 m_Velocity;
        protected Vector3 m_Accelerate;
        protected float m_MovingDelay = 0.0f;
        protected float m_MovingTime = 0.0f;
        protected Vector3 m_ImpactSrcPos;
        protected float m_ImpactSrcDir;
        protected TypedDataCollection m_CustomDatas = new TypedDataCollection();
        // 分段动作，定帧，位移信息
        protected CurveInfo m_AnimationInfo;
        protected CurveInfo m_LockFrameInfo;
        protected CurveMoveInfo m_MovementInfo;
        protected Quaternion m_MoveDir = Quaternion.identity;
        // 特效信息
        protected List<EffectInfo> m_EffectList = new List<EffectInfo>();
        protected List<GameObject> m_EffectsDelWithImpact = new List<GameObject>();
        // layer
    }

    public class EffectInfo : IPoolAllocatedObject<EffectInfo>
    {
        private EffectInfo m_DowncastObj = null;
        private ObjectPool<EffectInfo> m_Pool = null;

        public void Recycle()
        {
            Reset();
            if (null != m_Pool)
            {
                m_Pool.Recycle(this);
            }
        }

        public void InitPool(ObjectPool<EffectInfo> pool)
        {
            m_DowncastObj = this as EffectInfo;
            m_Pool = pool;
        }

        public EffectInfo Downcast()
        {
            return m_DowncastObj;
        }

        public void Reset()
        {
            ActorId = 0;
            IsActive = false;
            Path = string.Empty;
            StartTime = -1.0f;
            DelayTime = 0.0f;
            PlayTime = 0.0f;
            MountPoint = string.Empty;
            DelWithImpact = false;
            RelativePoint = Vector3.zero;
            RotateWithTarget = false;
            RelativeRotation = Vector3.zero;
        }



        public int ActorId;
        public bool IsActive;
        public string Path;
        public float StartTime = -1.0f;
        public float DelayTime = 0.0f;
        public float PlayTime = 0.0f;
        public string MountPoint;
        public bool DelWithImpact = false;
        public Vector3 RelativePoint = Vector3.zero;
        public bool RotateWithTarget = false;
        public Vector3 RelativeRotation = Vector3.zero;
    }
    public class CurveInfo
    {
        private class Section
        {
            public float StartTime = 0.0f;
            public float EndTime = 0.0f;
            public float Speed = 1.0f;
        }

        private List<Section> m_Sections = new List<Section>();

        public CurveInfo(string data)
        {
            List<float> floatData = Converter.ConvertNumericList<float>(data);
            if (floatData.Count > 0)
            {
                float curStartTime = floatData[0];
                for (int i = 1; i <= (floatData.Count - 1) / 2; ++i)
                {
                    Section sec = new Section();
                    sec.StartTime = curStartTime;
                    sec.EndTime = curStartTime + floatData[2 * i - 1];
                    sec.Speed = floatData[2 * i];
                    curStartTime += floatData[2 * i - 1];
                    m_Sections.Add(sec);
                }
            }
        }

        public virtual float GetSpeedByTime(float time)
        {
            for (int i = 0; i < m_Sections.Count; i++)
            {
                if (time >= m_Sections[i].StartTime && time < m_Sections[i].EndTime)
                {
                    return m_Sections[i].Speed;
                }
            }
            /*
            foreach (Section sec in m_Sections) {
                if (time >= sec.StartTime && time < sec.EndTime)
                {
                return sec.Speed;
              }
            }*/
            return 1.0f;
        }
    }



    public class CurveMoveInfo
    {
        private class Section
        {
            public float StartTime = 0.0f;
            public float EndTime = 0.0f;
            public float XSpeed = 0.0f;
            public float YSpeed = 0.0f;
            public float ZSpeed = 0.0f;
            public float XAcce = 0.0f;
            public float YAcce = 0.0f;
            public float ZAcce = 0.0f;
        }

        private List<Section> m_Sections = new List<Section>();

        public CurveMoveInfo(string data)
        {
            List<float> floatData = Converter.ConvertNumericList<float>(data);
            if (floatData.Count > 0)
            {
                float curStartTime = floatData[0];
                for (int i = 1; i <= (floatData.Count - 1) / 7; ++i)
                {
                    Section sec = new Section();
                    sec.StartTime = curStartTime;
                    sec.EndTime = curStartTime + floatData[7 * i - 6];
                    sec.XSpeed = floatData[7 * i - 5];
                    sec.YSpeed = floatData[7 * i - 4];
                    sec.ZSpeed = floatData[7 * i - 3];
                    sec.XAcce = floatData[7 * i - 2];
                    sec.YAcce = floatData[7 * i - 1];
                    sec.ZAcce = floatData[7 * i];
                    curStartTime += floatData[7 * i - 6];
                    m_Sections.Add(sec);
                }
            }
        }

        public void Dump()
        {
            foreach (Section sec in m_Sections)
            {
                LogSystem.Error("Section StartTime = " + sec.StartTime +
                               " EndTime = " + sec.EndTime +
                               " XSpeed = " + sec.XSpeed +
                               " YSpeed = " + sec.YSpeed +
                               " ZSpeed = " + sec.ZSpeed +
                               " XAcce = " + sec.XAcce +
                               " YAcce = " + sec.YAcce +
                               " ZAcce = " + sec.ZAcce);
            }
        }

        public Vector3 GetSpeedByTime(float time, float gravity = 0.0f)
        {
            foreach (Section sec in m_Sections)
            {
                if (time >= sec.StartTime && time < sec.EndTime)
                {
                    return new Vector3(sec.XSpeed + sec.XAcce * (time - sec.StartTime),
                                       sec.YSpeed + (sec.YAcce + gravity) * (time - sec.StartTime),
                                       sec.ZSpeed + sec.ZAcce * (time - sec.StartTime));
                }
            }
            if (time < GetStartTime())
            {
                return new Vector3(0, gravity * time, 0);
            }
            if (time > GetEndTime())
            {
                return new Vector3(0, (time - GetEndTime()) * gravity, 0);
            }

            return Vector3.zero;
        }

        private float GetStartTime()
        {
            if (m_Sections.Count > 0)
            {
                return m_Sections[0].StartTime;
            }
            return 0.0f;
        }

        private float GetEndTime()
        {
            if (m_Sections.Count > 0)
            {
                return m_Sections[m_Sections.Count - 1].EndTime;
            }
            return 0.0f;
        }
    }

    public class ImpactCacheData
    {
        public CurveInfo CurveAnimationInfo = null;
        public CurveInfo CurveLockFrameInfo = null;
        public CurveMoveInfo CurveMovementInfo = null;
        public List<float> MoveInfolist = null;
        public Vector3 AdjustPointV3 = Vector3.zero;

    };
}