namespace ArkCrossEngine
{
    public static class ResourceSystem
    {
        public static void PreloadResource(string res, int count)
        {
            ResourceManager.Instance.PreloadResource(res, count);
        }
        public static void PreloadResource(Object prefab, int count)
        {
            ResourceManager.Instance.PreloadResource(prefab, count);
        }
        public static void PreloadSharedResource(string res)
        {
            ResourceManager.Instance.PreloadSharedResource(res);
        }
        public static Object NewObject(string res)
        {
            return ResourceManager.Instance.NewObject(res);
        }
        public static Object NewObject(string res, float timeToRecycle)
        {
            return ResourceManager.Instance.NewObject(res, timeToRecycle);
        }
        public static Object NewObject(Object prefab)
        {
            return ResourceManager.Instance.NewObject(prefab);
        }
        public static Object NewObject(Object prefab, float timeToRecycle)
        {
            return ResourceManager.Instance.NewObject(prefab, timeToRecycle);
        }
        public static bool RecycleObject(Object obj)
        {
            return ResourceManager.Instance.RecycleObject(obj);
        }
        public static Object GetSharedResource(string res, bool isUseAssetbundle = true)
        {
            return ResourceManager.Instance.GetSharedResource(res, isUseAssetbundle);
        }
        public static void Cleanup()
        {
            ResourceManager.Instance.CleanupResourcePool();
        }
    }
}
