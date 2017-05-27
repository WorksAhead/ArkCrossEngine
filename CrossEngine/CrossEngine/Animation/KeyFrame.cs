using System.Runtime.InteropServices;

#region NameSpaceImplDecl
    #if UNITY_IMPL
        using CrossEngineImpl = UnityEngine;
    #elif UNREAL_IMPL
        using CrossEngineImpl = UnrealEngine;
    #else
        using CrossEngineImpl = ArkCrossEngine;
    #endif
#endregion

namespace ArkCrossEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public class Keyframe
    {
        public float time
        {
            get { return KeyframeImpl.time; }
            set { KeyframeImpl.time = value; }
        }
        public float value
        {
            get { return KeyframeImpl.value; }
            set { KeyframeImpl.value = value; }
        }
        public float inTangent
        {
            get { return KeyframeImpl.inTangent; }
            set { KeyframeImpl.value = value; }
        }
        public float outTangent
        {
            get { return KeyframeImpl.outTangent; }
            set { KeyframeImpl.outTangent = value; }
        }

        public Keyframe(CrossEngineImpl.Keyframe frame)
        {
            KeyframeImpl = frame;
        }
        public Keyframe(float time, float value)
        {
            KeyframeImpl = new CrossEngineImpl.Keyframe(time, value);
        }
        public Keyframe(float time, float value, float inTangent, float outTangent)
        {
            KeyframeImpl = new CrossEngineImpl.Keyframe(time, value, inTangent, outTangent);
        }

        public CrossEngineImpl.Keyframe KeyframeImpl;
    }
}
