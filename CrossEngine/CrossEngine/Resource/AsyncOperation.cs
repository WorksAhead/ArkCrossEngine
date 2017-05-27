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
    public class AsyncOperation
    {
        public AsyncOperation()
        {

        }

        public AsyncOperation(CrossEngineImpl.AsyncOperation AsyncOp)
        {
            AsyncOperationImpl = AsyncOp;
        }

        public bool allowSceneActivation
        {
            get { return AsyncOperationImpl.allowSceneActivation; }
        }
        public bool isDone
        {
            get { return AsyncOperationImpl.isDone; }
        }
        public int priority
        {
            get { return AsyncOperationImpl.priority; }
        }
        public float progress
        {
            get { return AsyncOperationImpl.progress; }
        }

        public CrossEngineImpl.AsyncOperation AsyncOperationImpl;
    }
}
