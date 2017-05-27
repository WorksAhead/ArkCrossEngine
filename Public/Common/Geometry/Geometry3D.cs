/**
 * @file MathUtil.cs
 * @brief 数学工具
 */
namespace ArkCrossEngine
{
    public class Geometry3D
    {
        public static Vector3 GetCenter(Vector3 fvPos1, Vector3 fvPos2)
        {
            Vector3 fvRet = new Vector3();

            fvRet.X = (fvPos1.X + fvPos2.X) / 2.0f;
            fvRet.Y = (fvPos1.Y + fvPos2.Y) / 2.0f;
            fvRet.Z = (fvPos1.Z + fvPos2.Z) / 2.0f;

            return fvRet;
        }
    }
}

