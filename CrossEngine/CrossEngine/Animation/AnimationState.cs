using System.ComponentModel;

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
    public class AnimationState
    {
        public AnimationState(CrossEngineImpl.AnimationState state)
        {
            AnimationStateImpl = state;
        }

        public ArkCrossEngine.AnimationBlendMode blendMode
        {
            get
            {
                return Convert(AnimationStateImpl.blendMode);
            }
            set
            {
                AnimationStateImpl.blendMode = (CrossEngineImpl.AnimationBlendMode)(int)value;
            }
        }
        public bool enabled
        {
            get { return AnimationStateImpl.enabled; }
            set { AnimationStateImpl.enabled = value; }
        }
        public int layer
        {
            get { return AnimationStateImpl.layer; }
            set { AnimationStateImpl.layer = value; }
        }
        public float length
        {
            get { return AnimationStateImpl.length; }
        }
        public string name
        {
            get { return AnimationStateImpl.name; }
            set { AnimationStateImpl.name = value; }
        }
        public float normalizedSpeed
        {
            get { return AnimationStateImpl.normalizedSpeed; }
            set { AnimationStateImpl.normalizedSpeed = value; }
        }
        public float normalizedTime
        {
            get { return AnimationStateImpl.normalizedTime; }
            set { AnimationStateImpl.normalizedTime = value; }
        }
        public float speed
        {
            get { return AnimationStateImpl.speed; }
            set { AnimationStateImpl.speed = value; }
        }
        public float time
        {
            get { return AnimationStateImpl.time; }
            set { AnimationStateImpl.time = value; }
        }
        public float weight
        {
            get { return AnimationStateImpl.weight; }
            set { AnimationStateImpl.weight = value; }
        }
        public ArkCrossEngine.WrapMode wrapMode
        {
            get { return Convert(AnimationStateImpl.wrapMode); }
            set { AnimationStateImpl.wrapMode = Convert(wrapMode); }
        }

        public void AddMixingTransform(Transform mix)
        {
            AnimationStateImpl.AddMixingTransform(mix.GetImpl<CrossEngineImpl.Transform>());
        }
        public void AddMixingTransform(Transform mix, bool recursive)
        {
            AnimationStateImpl.AddMixingTransform(mix.GetImpl<CrossEngineImpl.Transform>(), recursive);
        }
        public void RemoveMixingTransform(Transform mix)
        {
            AnimationStateImpl.RemoveMixingTransform(mix.GetImpl<CrossEngineImpl.Transform>());
        }

        private static ArkCrossEngine.AnimationBlendMode Convert(CrossEngineImpl.AnimationBlendMode mode)
        {
            return (ArkCrossEngine.AnimationBlendMode)(int)(CrossEngineImpl.AnimationBlendMode)(mode);
        }
        private static ArkCrossEngine.WrapMode Convert(CrossEngineImpl.WrapMode mode)
        {
            return (ArkCrossEngine.WrapMode)(int)(CrossEngineImpl.WrapMode)(mode);
        }
        private static CrossEngineImpl.WrapMode Convert(ArkCrossEngine.WrapMode mode)
        {
            return (CrossEngineImpl.WrapMode)(int)(ArkCrossEngine.WrapMode)(mode);
        }

        CrossEngineImpl.AnimationState AnimationStateImpl;
    }
}
