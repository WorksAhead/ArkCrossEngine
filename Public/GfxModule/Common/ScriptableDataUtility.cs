using ArkCrossEngine;

namespace GfxModule
{
    public static class ScriptableDataUtility
    {
        public static Vector2 CalcVector2(ScriptableData.CallData callData)
        {
            if (null == callData || callData.GetId() != "vector2")
                return Vector2.zero;
            int num = callData.GetParamNum();
            if (2 == num)
            {
                float x = float.Parse(callData.GetParamId(0));
                float y = float.Parse(callData.GetParamId(1));
                return new Vector2(x, y);
            }
            else
            {
                return Vector2.zero;
            }
        }
        public static UnityEngine.Vector3 CalcVector3(ScriptableData.CallData callData)
        {
            if (null == callData || callData.GetId() != "vector3")
                return UnityEngine.Vector3.zero;
            int num = callData.GetParamNum();
            if (3 == num)
            {
                float x = float.Parse(callData.GetParamId(0));
                float y = float.Parse(callData.GetParamId(1));
                float z = float.Parse(callData.GetParamId(2));
                return new UnityEngine.Vector3(x, y, z);
            }
            else
            {
                return UnityEngine.Vector3.zero;
            }
        }
        public static Vector4 CalcVector4(ScriptableData.CallData callData)
        {
            if (null == callData || callData.GetId() != "vector4")
                return Vector4.zero;
            int num = callData.GetParamNum();
            if (4 == num)
            {
                float x = float.Parse(callData.GetParamId(0));
                float y = float.Parse(callData.GetParamId(1));
                float z = float.Parse(callData.GetParamId(2));
                float w = float.Parse(callData.GetParamId(3));
                return new Vector4(x, y, z, w);
            }
            else
            {
                return Vector4.zero;
            }
        }
        public static UnityEngine.Color CalcColor(ScriptableData.CallData callData)
        {
            if (null == callData || callData.GetId() != "Color")
                return UnityEngine.Color.white;
            int num = callData.GetParamNum();
            if (4 == num)
            {
                float r = float.Parse(callData.GetParamId(0));
                float g = float.Parse(callData.GetParamId(1));
                float b = float.Parse(callData.GetParamId(2));
                float a = float.Parse(callData.GetParamId(3));
                return new UnityEngine.Color(r, g, b, a);
            }
            else
            {
                return UnityEngine.Color.white;
            }
        }
        public static Quaternion CalcQuaternion(ScriptableData.CallData callData)
        {
            if (null == callData || callData.GetId() != "quaternion")
                return Quaternion.identity;
            int num = callData.GetParamNum();
            if (4 == num)
            {
                float x = float.Parse(callData.GetParamId(0));
                float y = float.Parse(callData.GetParamId(1));
                float z = float.Parse(callData.GetParamId(2));
                float w = float.Parse(callData.GetParamId(3));
                return new Quaternion(x, y, z, w);
            }
            else
            {
                return Quaternion.identity;
            }
        }
        public static UnityEngine.Quaternion CalcEularRotation(ScriptableData.CallData callData)
        {
            if (null == callData || callData.GetId() != "eular")
                return UnityEngine.Quaternion.identity;
            int num = callData.GetParamNum();
            if (3 == num)
            {
                float x = float.Parse(callData.GetParamId(0));
                float y = float.Parse(callData.GetParamId(1));
                float z = float.Parse(callData.GetParamId(2));
                return UnityEngine.Quaternion.Euler(x, y, z);
            }
            else
            {
                return UnityEngine.Quaternion.identity;
            }
        }
    }
}
