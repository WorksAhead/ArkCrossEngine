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
    public class AnimationCurve
    {
        Keyframe[] initializeKeyframes(int length, CrossEngineImpl.Keyframe[] frame)
        {
            Keyframe[] array = new Keyframe[length];
            for(int i = 0; i < length; ++i)
            {
                array[i] = new Keyframe(frame[i]);
            }
            return array;
        }

        public Keyframe[] keys
        {
            get
            {
                return AnimationCurveImpl.keys.Length > 0 ? initializeKeyframes(AnimationCurveImpl.keys.Length, AnimationCurveImpl.keys) : null;
            }
        }

        public float Evaluate(float time)
        {
            return AnimationCurveImpl.Evaluate(time);
        }

        public void AddKey(Keyframe kf)
        {
            AnimationCurveImpl.AddKey(kf.KeyframeImpl);
        }

        CrossEngineImpl.AnimationCurve AnimationCurveImpl = new CrossEngineImpl.AnimationCurve();
    }
}
