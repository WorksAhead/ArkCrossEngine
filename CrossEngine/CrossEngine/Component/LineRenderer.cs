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
    public class LineRenderer : Renderer
    {
        public LineRenderer()
        {

        }

        public void SetWidth(float start, float end)
        {
            GetImpl<CrossEngineImpl.LineRenderer>().startWidth = start;
            GetImpl<CrossEngineImpl.LineRenderer>().endWidth = end;
        }
        public void SetColors(ArkCrossEngine.Color start, ArkCrossEngine.Color end)
        {
            GetImpl<CrossEngineImpl.LineRenderer>().startColor = Helper.ColorToUnity(start);
            GetImpl<CrossEngineImpl.LineRenderer>().endColor = Helper.ColorToUnity(end);
        }
        public void SetVertexCount(int count)
        {
            GetImpl<CrossEngineImpl.LineRenderer>().numPositions = count;
        }
        public void SetPosition(int index, ArkCrossEngine.Vector3 position)
        {
            GetImpl<CrossEngineImpl.LineRenderer>().SetPosition(index, Helper.Vec3ToUnity(position));
        }
    }
}
