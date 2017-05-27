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
    public class ParticleSystem : Component
    {
        public ParticleSystem()
        {

        }

        public bool playOnAwake
        {
            get { return GetImpl<CrossEngineImpl.ParticleSystem>().main.playOnAwake; }
            set
            {
                var m = GetImpl<CrossEngineImpl.ParticleSystem>().main;
                m.playOnAwake = value;
            }
        }
        public float playbackSpeed
        {
            get { return GetImpl<CrossEngineImpl.ParticleSystem>().main.simulationSpeed; }
            set { var m = GetImpl<CrossEngineImpl.ParticleSystem>().main; m.simulationSpeed = value; }
        }

        public void Play()
        {
            GetImpl<CrossEngineImpl.ParticleSystem>().Play();
        }

        public void Stop()
        {
            GetImpl<CrossEngineImpl.ParticleSystem>().Stop();
        }

        public void Clear()
        {
            GetImpl<CrossEngineImpl.ParticleSystem>().Clear();
        }
    }
}
