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
    public class Application
    {
        public static string loadedLevelName
        {
            get { return CrossEngineImpl.SceneManagement.SceneManager.GetActiveScene().name; }
        }

        public static AsyncOperation LoadLevelAsync(int index)
        {
            return new AsyncOperation(CrossEngineImpl.SceneManagement.SceneManager.LoadSceneAsync(index));
        }

        public static AsyncOperation LoadLevelAsync(string levelName)
        {
            return new AsyncOperation(CrossEngineImpl.SceneManagement.SceneManager.LoadSceneAsync(levelName));
        }
    }
}
