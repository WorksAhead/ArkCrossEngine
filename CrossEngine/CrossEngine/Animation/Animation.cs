using System.Collections;

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
    public class Animation : Component, IEnumerable
    {
        public Animation()
        {

        }
        
        public AnimationState this[string name]
        {
            get { return GetImpl<CrossEngineImpl.Animation>()[name] != null ? new AnimationState(GetImpl<CrossEngineImpl.Animation>()[name]) : null; }
        }
        public bool Play(ArkCrossEngine.PlayMode mode)
        {
            return GetImpl<CrossEngineImpl.Animation>().Play(ConvertPlayMode(mode));
        }
        public bool Play(string animation, ArkCrossEngine.PlayMode mode)
        {
            return GetImpl<CrossEngineImpl.Animation>().Play(animation, ConvertPlayMode(mode));
        }
        public bool Play(string animation)
        {
            return GetImpl<CrossEngineImpl.Animation>().Play(animation);
        }
        public bool IsPlaying(string name)
        {
            return GetImpl<CrossEngineImpl.Animation>().IsPlaying(name);
        }

        public void Sample()
        {
            GetImpl<CrossEngineImpl.Animation>().Sample();
        }

        public void Stop()
        {
            GetImpl<CrossEngineImpl.Animation>().Stop();
        }

        public void Stop(string name)
        {
            GetImpl<CrossEngineImpl.Animation>().Stop(name);
        }
        
        public int GetClipCount()
        {
            return GetImpl<CrossEngineImpl.Animation>().GetClipCount();
        }

        public void Blend(string animation, float targetWeight, float addLoopFrame)
        {
            GetImpl<CrossEngineImpl.Animation>().Blend(animation, targetWeight, addLoopFrame);
        }

        public void CrossFade(string animation)
        {
            GetImpl<CrossEngineImpl.Animation>().CrossFade(animation);
        }
        public void CrossFade(string animation, float fadeLength)
        {
            GetImpl<CrossEngineImpl.Animation>().CrossFade(animation, fadeLength);
        }
        public void CrossFade(string animation, float fadeLength, ArkCrossEngine.PlayMode mode)
        {
            GetImpl<CrossEngineImpl.Animation>().CrossFade(animation, fadeLength, ConvertPlayMode(mode));
        }

        public AnimationState PlayQueued(string animation)
        {
            return new AnimationState(GetImpl<CrossEngineImpl.Animation>().PlayQueued(animation));
        }
        public AnimationState PlayQueued(string animation, ArkCrossEngine.QueueMode queue)
        {
            return new AnimationState(GetImpl<CrossEngineImpl.Animation>().PlayQueued(animation, ConvertQueueMode(queue)));
        }
        public AnimationState PlayQueued(string animation, ArkCrossEngine.QueueMode queue, ArkCrossEngine.PlayMode mode)
        {
            return new AnimationState(GetImpl<CrossEngineImpl.Animation>().PlayQueued(animation, ConvertQueueMode(queue), ConvertPlayMode(mode)));
        }

        public AnimationState CrossFadeQueued(string animation)
        {
            return new AnimationState(GetImpl<CrossEngineImpl.Animation>().CrossFadeQueued(animation));
        }
        public AnimationState CrossFadeQueued(string animation, float fadeLength)
        {
            return new AnimationState(GetImpl<CrossEngineImpl.Animation>().CrossFadeQueued(animation, fadeLength));
        }
        public AnimationState CrossFadeQueued(string animation, float fadeLength, ArkCrossEngine.QueueMode queue, ArkCrossEngine.PlayMode mode)
        {
            return new AnimationState(GetImpl<CrossEngineImpl.Animation>().CrossFadeQueued(animation, fadeLength, ConvertQueueMode(queue), ConvertPlayMode(mode)));
        }

        public void Rewind()
        {
            GetImpl<CrossEngineImpl.Animation>().Rewind();
        }
        public void Rewind(string name)
        {
            GetImpl<CrossEngineImpl.Animation>().Rewind(name);
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        private sealed partial class Enumerator : IEnumerator
        {
            private Animation m_Outer;

            internal Enumerator(Animation outer) { m_Outer = outer; }
            public object Current
            {
                get
                {
                    return new AnimationState((CrossEngineImpl.AnimationState)m_Outer.GetImpl<CrossEngineImpl.Animation>().GetEnumerator().Current);
                }
            }

            public bool MoveNext()
            {
                return m_Outer.GetImpl<CrossEngineImpl.Animation>().GetEnumerator().MoveNext();
            }

            public void Reset() { m_Outer.GetImpl<CrossEngineImpl.Animation>().GetEnumerator().Reset(); }
        }

        private static CrossEngineImpl.PlayMode ConvertPlayMode(ArkCrossEngine.PlayMode mode)
        {
            return (CrossEngineImpl.PlayMode)(int)(ArkCrossEngine.PlayMode)(mode);
        }
        private static CrossEngineImpl.QueueMode ConvertQueueMode(ArkCrossEngine.QueueMode mode)
        {
            return (CrossEngineImpl.QueueMode)(int)(ArkCrossEngine.QueueMode)(mode);
        }
    }
}
