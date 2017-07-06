using System;
using System.Collections.Generic;
using ArkCrossEngine;
namespace DashFire
{
  internal static class ScriptableDataUtility
  {
    internal static Vector2 CalcVector2(ScriptableData.CallData callData)
    {
      if (null == callData || callData.GetId() != "vector2")
        return Vector2.zero;
      int num = callData.GetParamNum();
      if (2 == num) {
        float x = float.Parse(callData.GetParamId(0));
        float y = float.Parse(callData.GetParamId(1));
        return new Vector2(x, y);
      } else {
        return Vector2.zero;
      }
    }
    internal static Vector3 CalcVector3(ScriptableData.CallData callData)
    {
      if (null == callData || callData.GetId() != "vector3")
        return Vector3.Zero;
      int num = callData.GetParamNum();
      if (3 == num) {
        float x = float.Parse(callData.GetParamId(0));
        float y = float.Parse(callData.GetParamId(1));
        float z = float.Parse(callData.GetParamId(2));
        return new Vector3(x, y, z);
      } else {
        return Vector3.Zero;
      }
    }
    internal static Vector4 CalcVector4(ScriptableData.CallData callData)
    {
      if (null == callData || callData.GetId() != "vector4")
        return Vector4.Zero;
      int num = callData.GetParamNum();
      if (4 == num) {
        float x = float.Parse(callData.GetParamId(0));
        float y = float.Parse(callData.GetParamId(1));
        float z = float.Parse(callData.GetParamId(2));
        float w = float.Parse(callData.GetParamId(3));
        return new Vector4(x, y, z, w);
      } else {
        return Vector4.Zero;
      }
    }
    internal static Quaternion CalcQuaternion(ScriptableData.CallData callData)
    {
      if (null == callData || callData.GetId() != "quaternion")
        return Quaternion.identity;
      int num = callData.GetParamNum();
      if (4 == num) {
        float x = float.Parse(callData.GetParamId(0));
        float y = float.Parse(callData.GetParamId(1));
        float z = float.Parse(callData.GetParamId(2));
        float w = float.Parse(callData.GetParamId(3));
        return new Quaternion(x, y, z, w);
      } else {
        return Quaternion.identity;
      }
    }
    internal static Quaternion CalcEularRotation(ScriptableData.CallData callData)
    {
      if (null == callData || callData.GetId() != "eular")
        return Quaternion.identity;
      int num = callData.GetParamNum();
      if (3 == num) {
        float x = float.Parse(callData.GetParamId(0));
        float y = float.Parse(callData.GetParamId(1));
        float z = float.Parse(callData.GetParamId(2));
        return Quaternion.CreateFromYawPitchRoll(x, y, z);
      } else {
        return Quaternion.identity;
      }
    }
  }
}
