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
    public class AudioSource : Component
    {
        public AudioSource()
        {

        }

        public AudioClip clip
        {
            get { return ObjectFactory.Create<AudioClip>(GetImpl<CrossEngineImpl.AudioSource>().clip); }
            set
            {
                if (value != null)
                {
                    GetImpl<CrossEngineImpl.AudioSource>().clip = value.GetImpl<CrossEngineImpl.AudioClip>();
                }
                else
                {
                    GetImpl<CrossEngineImpl.AudioSource>().clip = null;
                }
            }
        }
        public float dopplerLevel
        {
            get { return GetImpl<CrossEngineImpl.AudioSource>().dopplerLevel; }
            set { GetImpl<CrossEngineImpl.AudioSource>().dopplerLevel = value; }
        }
        public float pitch
        {
            get { return GetImpl<CrossEngineImpl.AudioSource>().pitch; }
            set { GetImpl<CrossEngineImpl.AudioSource>().pitch = value; }
        }
        public bool loop
        {
            get { return GetImpl<CrossEngineImpl.AudioSource>().loop; }
            set { GetImpl<CrossEngineImpl.AudioSource>().loop = value; }
        }
        public float volume
        {
            get { return GetImpl<CrossEngineImpl.AudioSource>().volume; }
            set { GetImpl<CrossEngineImpl.AudioSource>().volume = value; }
        }
        public void Play()
        {
            GetImpl<CrossEngineImpl.AudioSource>().Play();
        }
        public void Stop()
        {
            GetImpl<CrossEngineImpl.AudioSource>().Stop();
        }
        public void PlayOneShot(AudioClip clip)
        {
            GetImpl<CrossEngineImpl.AudioSource>().PlayOneShot(clip.GetImpl<CrossEngineImpl.AudioClip>());
        }
    }
}
