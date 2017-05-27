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
    public sealed class Time
    {
        public static int captureFramerate
        {
            get { return CrossEngineImpl.Time.captureFramerate; }
        }
        public static float deltaTime
        {
            get { return CrossEngineImpl.Time.deltaTime; }
        }
        public static float fixedDeltaTime
        {
            get { return CrossEngineImpl.Time.fixedDeltaTime; }
            set { ArkCrossEngine.Time.fixedDeltaTime = value; }
        }
        public static float fixedTime
        {
            get { return CrossEngineImpl.Time.fixedTime; }
        }
        public static int frameCount
        {
            get { return CrossEngineImpl.Time.frameCount; }
        }
        public static float maximumDeltaTime
        {
            get { return CrossEngineImpl.Time.maximumDeltaTime; }
        }
        public static float realtimeSinceStartup
        {
            get { return CrossEngineImpl.Time.realtimeSinceStartup; }
        }
        public static int renderedFrameCount
        {
            get { return CrossEngineImpl.Time.renderedFrameCount; }
        }
        public static float smoothDeltaTime
        {
            get { return CrossEngineImpl.Time.smoothDeltaTime; }
        }
        public static float time
        {
            get { return CrossEngineImpl.Time.time; }
        }
        public static float timeScale
        {
            get { return CrossEngineImpl.Time.timeScale; }
            set { CrossEngineImpl.Time.timeScale = value; }
        }
        public static float timeSinceLevelLoad
        {
            get { return CrossEngineImpl.Time.timeSinceLevelLoad; }
        }
    }
}
