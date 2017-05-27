using System;
using System.Runtime.InteropServices;

namespace ArkCrossEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Mathf
    {
        public const float PI = (float)Math.PI;

        public const float Infinity = Single.PositiveInfinity;

        public const float NegativeInfinity = Single.NegativeInfinity;

        public const float Deg2Rad = PI * 2F / 360F;

        public const float Rad2Deg = 1F / Deg2Rad;

        public const float Epsilon = Single.Epsilon;

        public static float Sin(float f) { return (float)Math.Sin(f); }
        public static float Cos(float f) { return (float)Math.Cos(f); }
        public static float Tan(float f) { return (float)Math.Tan(f); }
        public static float Asin(float f) { return (float)Math.Asin(f); }
        public static float Acos(float f) { return (float)Math.Acos(f); }
        public static float Atan(float f) { return (float)Math.Atan(f); }
        public static float Atan2(float y, float x) { return (float)Math.Atan2(y, x); }
        public static float Sqrt(float f) { return (float)Math.Sqrt(f); }
        public static float Abs(float f) { return (float)Math.Abs(f); }
        public static int Abs(int value) { return Math.Abs(value); }
        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }
        public static float Min(params float[] values)
        {
            int len = values.Length;
            if (len == 0)
                return 0;
            float m = values[0];
            for (int i = 1; i < len; i++)
            {
                if (values[i] < m)
                    m = values[i];
            }
            return m;
        }
        public static int Min(int a, int b)
        {
            return a < b ? a : b;
        }
        public static int Min(params int[] values)
        {
            int len = values.Length;
            if (len == 0)
                return 0;
            int m = values[0];
            for (int i = 1; i < len; i++)
            {
                if (values[i] < m)
                    m = values[i];
            }
            return m;
        }
        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }
        public static float Max(params float[] values)
        {
            int len = values.Length;
            if (len == 0)
                return 0;
            float m = values[0];
            for (int i = 1; i < len; i++)
            {
                if (values[i] > m)
                    m = values[i];
            }
            return m;
        }
        public static int Max(int a, int b)
        {
            return a > b ? a : b;
        }
        public static int Max(params int[] values)
        {
            int len = values.Length;
            if (len == 0)
                return 0;
            int m = values[0];
            for (int i = 1; i < len; i++)
            {
                if (values[i] > m)
                    m = values[i];
            }
            return m;
        }
        public static float Pow(float f, float p) { return (float)Math.Pow(f, p); }
        public static float Exp(float power) { return (float)Math.Exp(power); }
        public static float Log(float f, float p) { return (float)Math.Log(f, p); }
        public static float Log(float f) { return (float)Math.Log(f); }
        public static float Log10(float f) { return (float)Math.Log10(f); }
        public static float Ceil(float f) { return (float)Math.Ceiling(f); }
        public static float Floor(float f) { return (float)Math.Floor(f); }
        public static float Round(float f) { return (float)Math.Round(f); }
        public static int CeilToInt(float f) { return (int)Math.Ceiling(f); }
        public static int FloorToInt(float f) { return (int)Math.Floor(f); }
        public static int RoundToInt(float f) { return (int)Math.Round(f); }
        public static float Sign(float f) { return f >= 0F ? 1F : -1F; }
        
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }
        
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }
        
        public static float Clamp01(float value)
        {
            if (value < 0F)
                return 0F;
            else if (value > 1F)
                return 1F;
            else
                return value;
        }

        public static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * Clamp01(t);
        }
        
        public static float LerpAngle(float a, float b, float t)
        {
            float delta = Repeat((b - a), 360);
            if (delta > 180)
                delta -= 360;
            return a + delta * Clamp01(t);
        }

        static public float MoveTowards(float current, float target, float maxDelta)
        {
            if (Mathf.Abs(target - current) <= maxDelta)
                return target;
            return current + Mathf.Sign(target - current) * maxDelta;
        }

        static public float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            target = current + DeltaAngle(current, target);
            return MoveTowards(current, target, maxDelta);
        }

        public static float SmoothStep(float from, float to, float t)
        {
            t = Mathf.Clamp01(t);
            t = -2.0F * t * t * t + 3.0F * t * t;
            return to * t + from * (1F - t);
        }
        
        public static float Gamma(float value, float absmax, float gamma)
        {
            bool negative = false;
            if (value < 0F)
                negative = true;
            float absval = Abs(value);
            if (absval > absmax)
                return negative ? -absval : absval;

            float result = Pow(absval / absmax, gamma) * absmax;
            return negative ? -result : result;
        }

        public static bool Approximately(float a, float b)
        {
            return Abs(b - a) < Max(0.000001f * Max(Abs(a), Abs(b)), Epsilon * 8);
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            smoothTime = Mathf.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float change = current - target;
            float originalTo = target;

            float maxChange = maxSpeed * smoothTime;
            change = Mathf.Clamp(change, -maxChange, maxChange);
            target = current - change;

            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;

            if (originalTo - current > 0.0F == output > originalTo)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

            return output;
        }

        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            target = current + DeltaAngle(current, target);
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static float Repeat(float t, float length)
        {
            return t - Mathf.Floor(t / length) * length;
        }

        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2F);
            return length - Mathf.Abs(t - length);
        }

        public static float InverseLerp(float from, float to, float value)
        {
            if (from < to)
            {
                if (value < from)
                    return 0.0F;
                else if (value > to)
                    return 1.0F;
                else
                {
                    value -= from;
                    value /= (to - from);
                    return value;
                }
            }
            else if (from > to)
            {
                if (value < to)
                    return 1.0F;
                else if (value > from)
                    return 0.0F;
                else
                {
                    return 1.0F - ((value - to) / (from - to));
                }
            }
            else
            {
                return 0.0F;
            }
        }

        public static float DeltaAngle(float current, float target)
        {
            float delta = Mathf.Repeat((target - current), 360.0F);
            if (delta > 180.0F)
                delta -= 360.0F;
            return delta;
        }

        public static float DegreeToRadius(float deg)
        {
            return deg / 360.0f * 2.0f * (float)Math.PI;
        }

        public static float RadiusToDegree(float rad)
        {
            return rad / 2.0f / (float)Math.PI * 360.0f;
        }

        internal static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
        {
            float bx = p2.x - p1.x;
            float by = p2.y - p1.y;
            float dx = p4.x - p3.x;
            float dy = p4.y - p3.y;
            float bDotDPerp = bx * dy - by * dx;
            if (bDotDPerp == 0)
            {
                return false;
            }
            float cx = p3.x - p1.x;
            float cy = p3.y - p1.y;
            float t = (cx * dy - cy * dx) / bDotDPerp;

            result = new Vector2(p1.x + t * bx, p1.y + t * by);
            return true;
        }
        
        internal static bool LineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
        {
            float bx = p2.x - p1.x;
            float by = p2.y - p1.y;
            float dx = p4.x - p3.x;
            float dy = p4.y - p3.y;
            float bDotDPerp = bx * dy - by * dx;
            if (bDotDPerp == 0)
            {
                return false;
            }
            float cx = p3.x - p1.x;
            float cy = p3.y - p1.y;
            float t = (cx * dy - cy * dx) / bDotDPerp;
            if (t < 0 || t > 1)
            {
                return false;
            }
            float u = (cx * by - cy * bx) / bDotDPerp;
            if (u < 0 || u > 1)
            {
                return false;
            }
            result = new Vector2(p1.x + t * bx, p1.y + t * by);
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2
    {
        public static Vector2 zero { get { return new Vector2(0.0F, 0.0F); } }
        public static Vector2 one { get { return new Vector2(1.0F, 1.0F); } }
        public static Vector2 up { get { return new Vector2(0.0F, 1.0F); } }
        public static Vector2 right { get { return new Vector2(1.0F, 0.0F); } }
        
        public const float kEpsilon = 0.00001F;

        public float x;
        public float y;

        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                }
            }
        }

        public Vector2(float x, float y) { this.x = x; this.y = y; }

        public void Set(float new_x, float new_y) { x = new_x; y = new_y; }

        public static Vector2 Lerp(Vector2 from, Vector2 to, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector2(
                from.x + (to.x - from.x) * t,
                from.y + (to.y - from.y) * t
            );
        }

        static public Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
        {
            Vector2 toVector = target - current;
            float dist = toVector.magnitude;
            if (dist <= maxDistanceDelta || dist == 0)
                return target;
            return current + toVector / dist * maxDistanceDelta;
        }

        public static Vector2 Scale(Vector2 a, Vector2 b) { return new Vector2(a.x * b.x, a.y * b.y); }

        public void Scale(Vector2 scale) { x *= scale.x; y *= scale.y; }

        public void Normalize()
        {
            float mag = this.magnitude;
            if (mag > kEpsilon)
                this = this / mag;
            else
                this = zero;
        }

        public Vector2 normalized
        {
            get
            {
                Vector2 v = new Vector2(x, y);
                v.Normalize();
                return v;
            }
        }

        public float Length()
        {
            return Mathf.Sqrt(x * x + y * y);
        }

        override public string ToString()
        {
            return String.Format("({0:F1}, {1:F1})", x, y);
        }

        public string ToString(string format)
        {
            return String.Format("({0}, {1})", x.ToString(format), y.ToString(format));
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector2)) return false;

            Vector2 rhs = (Vector2)other;
            return x.Equals(rhs.x) && y.Equals(rhs.y);
        }

        public static float Dot(Vector2 lhs, Vector2 rhs) { return lhs.x * rhs.x + lhs.y * rhs.y; }
        
        public float magnitude { get { return Mathf.Sqrt(x * x + y * y); } }

        public float sqrMagnitude { get { return x * x + y * y; } }

        public static float Angle(Vector2 from, Vector2 to) { return Mathf.Acos(Mathf.Clamp(Dot(from.normalized, to.normalized), -1F, 1F)) * Mathf.Rad2Deg; }

        public static float Distance(Vector2 a, Vector2 b) { return (a - b).magnitude; }

        public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
        {
            if (vector.sqrMagnitude > maxLength * maxLength)
                return vector.normalized * maxLength;
            return vector;
        }

        public static float SqrMagnitude(Vector2 a) { return a.x * a.x + a.y * a.y; }

        public float SqrMagnitude() { return x * x + y * y; }

        public static Vector2 Min(Vector2 lhs, Vector2 rhs) { return new Vector2(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y)); }

        public static Vector2 Max(Vector2 lhs, Vector2 rhs) { return new Vector2(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y)); }

        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator -(Vector2 a) { return new Vector2(-a.x, -a.y); }
        public static Vector2 operator *(Vector2 a, float d) { return new Vector2(a.x * d, a.y * d); }
        public static Vector2 operator *(float d, Vector2 a) { return new Vector2(a.x * d, a.y * d); }
        public static Vector2 operator /(Vector2 a, float d) { return new Vector2(a.x / d, a.y / d); }
        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            return SqrMagnitude(lhs - rhs) < kEpsilon * kEpsilon;
        }
        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return SqrMagnitude(lhs - rhs) >= kEpsilon * kEpsilon;
        }

        /* Ambiguous
        public static implicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }
        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }
        */
    }

    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector3
    {
        public const float kEpsilon = 0.00001F;
        public const float k1OverSqrt2 = 0.7071067811865475244008443621048490f;

        public float x;
        public float y;
        public float z;

        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }
        public float Z { get { return z; } set { z = value; } }

        public static Vector3 zero { get { return new Vector3(0F, 0F, 0F); } }
        public static Vector3 one { get { return new Vector3(1F, 1F, 1F); } }
        public static Vector3 forward { get { return new Vector3(0F, 0F, 1F); } }
        public static Vector3 back { get { return new Vector3(0F, 0F, -1F); } }
        public static Vector3 up { get { return new Vector3(0F, 1F, 0F); } }
        public static Vector3 down { get { return new Vector3(0F, -1F, 0F); } }
        public static Vector3 left { get { return new Vector3(-1F, 0F, 0F); } }
        public static Vector3 right { get { return new Vector3(1F, 0F, 0F); } }
        public static Vector3 Zero { get { return zero; } }

        public static float Lerp(float from, float to, float t)
        {
            return to * t + from * (1.0f - t);
        }

        public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector3(
                from.x + (to.x - from.x) * t,
                from.y + (to.y - from.y) * t,
                from.z + (to.z - from.z) * t
            );
        }

        public static Vector3 Slerp(Vector3 lhs, Vector3 rhs, float t)
        {
            t = Mathf.Clamp01(t);

            float lhsMag = Magnitude(lhs);
            float rhsMag = Magnitude(rhs);

            if (lhsMag < kEpsilon || rhsMag < kEpsilon)
                return Lerp(lhs, rhs, t);

            float lerpedMagnitude = Lerp(lhsMag, rhsMag, t);

            float dot = Dot(lhs, rhs) / (lhsMag * rhsMag);
            // direction is almost the same
            if (dot > 1.0F - kEpsilon)
            {
                return Lerp(lhs, rhs, t);
            }
            // directions are almost opposite
            else if (dot < -1.0F + kEpsilon)
            {
                Vector3 lhsNorm = lhs / lhsMag;
                Vector3 axis = OrthoNormalVectorFast(lhsNorm);
                Matrix4x4 m = Matrix4x4.SetAxisAngle(axis, Mathf.PI * t);
                Vector3 slerped = m.MultiplyVector(lhsNorm);
                slerped *= lerpedMagnitude;
                return slerped;
            }
            // normal case
            else
            {
                Vector3 axis = Cross(lhs, rhs);
                Vector3 lhsNorm = lhs / lhsMag;
                axis = Normalize(axis);
                float angle = Mathf.Acos(dot) * t;

                Matrix4x4 m = Matrix4x4.SetAxisAngle(axis, angle);
                Vector3 slerped = m.MultiplyVector(lhsNorm);
                slerped *= lerpedMagnitude;
                return slerped;
            }
        }

        public static Vector3 OrthoNormalVectorFast(Vector3 n)
        {
            Vector3 res;
            if (Mathf.Abs(n.z) > k1OverSqrt2)
            {
                // choose p in y-z plane
                float a = n.y * n.y + n.z * n.z;
                float k = 1.0F / Mathf.Sqrt(a);
                res.x = 0;
                res.y = -n.z * k;
                res.z = n.y * k;
            }
            else
            {
                // choose p in x-y plane
                float a = n.x * n.x + n.y * n.y;
                float k = 1.0F / Mathf.Sqrt(a);
                res.x = -n.y * k;
                res.y = n.x * k;
                res.z = 0;
            }
            return res;
        }

        static public void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent)
        {
            // compute u0
            float mag = Magnitude(normal);
            if (mag > kEpsilon)
            {
                normal /= mag;
            }
            else
            {
                normal = new Vector3(1.0f, 0.0f, 0.0f);
            }

            // compute u1
            float dot0 = Dot(normal, tangent);
            tangent -= dot0 * normal;
            mag = Magnitude(tangent);
            if (mag < kEpsilon)
            {
                tangent = OrthoNormalVectorFast(normal);
            }
            else
            {
                tangent /= mag;
            }
        }
        static public void OrthoNormalizeFast(ref Vector3 normal, ref Vector3 tangent, ref Vector3 binormal)
        {
            // compute u0
            normal = Normalize(normal);

            // compute u1
            float dot0 = Dot(normal, tangent);
            tangent -= dot0 * normal;
            tangent = Normalize(tangent);

            // compute u2
            float dot1 = Dot(tangent, binormal);
            dot0 = Dot(normal, binormal);
            binormal -= dot0 * normal + dot1 * tangent;
            binormal = Normalize(binormal);
        }
        static public void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent, ref Vector3 binormal)
        {
            // compute u0
            float mag = Magnitude(normal);
            if (mag > kEpsilon)
                normal /= mag;
            else
                normal = new Vector3(1.0F, 0.0F, 0.0F);

            // compute u1
            float dot0 = Dot(normal, tangent);
            tangent -= dot0 * normal;
            mag = Magnitude(tangent);
            if (mag > kEpsilon)
                tangent /= mag;
            else
                tangent = OrthoNormalVectorFast(normal);

            // compute u2
            float dot1 = Dot(tangent, binormal);
            dot0 = Dot(normal, binormal);
            binormal -= dot0 * normal + dot1 * tangent;
            mag = Magnitude(binormal);
            if (mag > kEpsilon)
                binormal /= mag;
            else
                binormal = Cross(normal, tangent);
        }

        static public Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            Vector3 toVector = target - current;
            float dist = toVector.magnitude;
            if (dist <= maxDistanceDelta || dist == 0)
                return target;
            return current + toVector / dist * maxDistanceDelta;
        }

        public static float ClampedMove(float lhs, float rhs, float clampedDelta)
        {
            float delta = rhs - lhs;
            if (delta > 0.0F)
                return lhs + Mathf.Min(delta, clampedDelta);
            else
                return lhs - Mathf.Min(-delta, clampedDelta);
        }

        public static Vector3 RotateTowards(Vector3 lhs, Vector3 rhs, float angleMove, float magnitudeMove)
        {
            float lhsMag = Magnitude(lhs);
            float rhsMag = Magnitude(rhs);

            // both vectors are non-zero
            if (lhsMag > Vector3.kEpsilon && rhsMag > Vector3.kEpsilon)
            {
                Vector3 lhsNorm = lhs / lhsMag;
                Vector3 rhsNorm = rhs / rhsMag;

                float dot = Dot(lhsNorm, rhsNorm);
                // direction is almost the same
                if (dot > 1.0F - Vector3.kEpsilon)
                {
                    return MoveTowards(lhs, rhs, magnitudeMove);
                }
                // directions are almost opposite
                else if (dot < -1.0F + Vector3.kEpsilon)
                {
                    Vector3 axis = OrthoNormalVectorFast(lhsNorm);
                    Matrix4x4 m = Matrix4x4.SetAxisAngle(axis, angleMove);
                    Vector3 rotated = m.MultiplyVector(lhsNorm);
                    rotated *= ClampedMove(lhsMag, rhsMag, magnitudeMove);
                    return rotated;
                }
                // normal case
                else
                {
                    float angle = Mathf.Acos(dot);
                    Vector3 axis = Normalize(Cross(lhsNorm, rhsNorm));
                    Matrix4x4 m = Matrix4x4.SetAxisAngle(axis, Math.Min(angleMove, angle));
                    Vector3 rotated = m.MultiplyVector(lhsNorm);
                    rotated *= ClampedMove(lhsMag, rhsMag, magnitudeMove);
                    return rotated;
                }
            }
            // at least one of the vectors is almost zero
            else
            {
                return MoveTowards(lhs, rhs, magnitudeMove);
            }
        }

        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            smoothTime = Mathf.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            Vector3 change = current - target;
            Vector3 originalTo = target;

            float maxChange = maxSpeed * smoothTime;
            change = ClampMagnitude(change, maxChange);
            target = current - change;

            Vector3 temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            Vector3 output = target + (change + temp) * exp;

            if (Dot(originalTo - current, output - originalTo) > 0)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

            return output;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

        public Vector3(float x, float y) { this.x = x; this.y = y; z = 0F; }

        public void Set(float new_x, float new_y, float new_z) { x = new_x; y = new_y; z = new_z; }

        public static Vector3 Scale(Vector3 a, Vector3 b) { return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z); }

        public void Scale(Vector3 scale) { x *= scale.x; y *= scale.y; z *= scale.z; }

        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(
            lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3)) return false;

            Vector3 rhs = (Vector3)other;
            return x.Equals(rhs.x) && y.Equals(rhs.y) && z.Equals(rhs.z);
        }

        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
        {
            return -2F * Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            float mag = Magnitude(value);
            if (mag > kEpsilon)
                return value / mag;
            else
                return zero;
        }

        public void Normalize()
        {
            float mag = Magnitude(this);
            if (mag > kEpsilon)
                this = this / mag;
            else
                this = zero;
        }

        public Vector3 normalized { get { return Vector3.Normalize(this); } }
        
        override public string ToString()
        {
            return String.Format("({0:F1}, {1:F1}, {2:F1})", x, y, z);
        }
        public string ToString(string format)
        {
            return String.Format("({0}, {1}, {2})", x.ToString(format), y.ToString(format), z.ToString(format));
        }

        public static float Dot(Vector3 lhs, Vector3 rhs) { return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z; }

        public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        {
            float sqrMag = Dot(onNormal, onNormal);
            if (sqrMag < Mathf.Epsilon)
                return zero;
            else
                return onNormal * Dot(vector, onNormal) / sqrMag;
        }

        public static Vector3 Exclude(Vector3 excludeThis, Vector3 fromThat)
        {
            return fromThat - Project(fromThat, excludeThis);
        }

        public static float Angle(Vector3 from, Vector3 to) { return Mathf.Acos(Mathf.Clamp(Vector3.Dot(from.normalized, to.normalized), -1F, 1F)) * Mathf.Rad2Deg; }

        public static float Distance(Vector3 a, Vector3 b) { Vector3 vec = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); return Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z); }

        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
            if (vector.sqrMagnitude > maxLength * maxLength)
                return vector.normalized * maxLength;
            return vector;
        }

        public static float Magnitude(Vector3 a) { return Mathf.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z); }

        public float magnitude { get { return Mathf.Sqrt(x * x + y * y + z * z); } }

        public static float SqrMagnitude(Vector3 a) { return a.x * a.x + a.y * a.y + a.z * a.z; }

        public float sqrMagnitude { get { return x * x + y * y + z * z; } }

        public static Vector3 Min(Vector3 lhs, Vector3 rhs) { return new Vector3(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z)); }

        public static Vector3 Max(Vector3 lhs, Vector3 rhs) { return new Vector3(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z)); }
        
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vector3 operator -(Vector3 a) { return new Vector3(-a.x, -a.y, -a.z); }
        public static Vector3 operator *(Vector3 a, float d) { return new Vector3(a.x * d, a.y * d, a.z * d); }
        public static Vector3 operator *(float d, Vector3 a) { return new Vector3(a.x * d, a.y * d, a.z * d); }
        public static Vector3 operator /(Vector3 a, float d) { return new Vector3(a.x / d, a.y / d, a.z / d); }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return SqrMagnitude(lhs - rhs) < kEpsilon * kEpsilon;
        }

        public float Length()
        {
            return Mathf.Sqrt(LengthSquared());
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return SqrMagnitude(lhs - rhs) >= kEpsilon * kEpsilon;
        }

        public float LengthSquared()
        {
            return x * x + y * y + z * z;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static Color red { get { return new Color(1F, 0F, 0F, 1F); } }
        public static Color green { get { return new Color(0F, 1F, 0F, 1F); } }
        public static Color blue { get { return new Color(0F, 0F, 1F, 1F); } }
        public static Color white { get { return new Color(1F, 1F, 1F, 1F); } }
        public static Color black { get { return new Color(0F, 0F, 0F, 1F); } }
        public static Color yellow { get { return new Color(1F, 235F / 255F, 4F / 255F, 1F); } }
        public static Color cyan { get { return new Color(0F, 1F, 1F, 1F); } }
        public static Color magenta { get { return new Color(1F, 0F, 1F, 1F); } }
        public static Color gray { get { return new Color(.5F, .5F, .5F, 1F); } }
        public static Color grey { get { return new Color(.5F, .5F, .5F, 1F); } }
        public static Color clear { get { return new Color(0F, 0F, 0F, 0F); } }

        public float grayscale { get { return 0.299F * r + 0.587F * g + 0.114F * b; } }

        public Color linear
        {
            get
            {
                return new Color(r, g, b, a);
            }
        }

        public Color gamma
        {
            get
            {
                return new Color(r, g, b, a);
            }
        }

        public static implicit operator Vector4(Color c)
        {
            return new Vector4(c.r, c.g, c.b, c.a);
        }
        public static implicit operator Color(Vector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return r;
                    case 1: return g;
                    case 2: return b;
                    case 3: return a;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: r = value; break;
                    case 1: g = value; break;
                    case 2: b = value; break;
                    case 3: a = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public Color(float r, float g, float b, float a)
        {
            this.r = r; this.g = g; this.b = b; this.a = a;
        }

        public Color(float r, float g, float b)
        {
            this.r = r; this.g = g; this.b = b; this.a = 1.0F;
        }
        
        override public string ToString()
        {
            return String.Format("RGBA({0:F3}, {1:F3}, {2:F3}, {3:F3})", r, g, b, a);
        }
        public string ToString(string format)
        {
            return String.Format("RGBA({0}, {1}, {2}, {3})", r.ToString(format), g.ToString(format), b.ToString(format), a.ToString(format));
        }

        public override int GetHashCode()
        {
            return ((Vector4)this).GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (!(other is Color)) return false;
            Color rhs = (Color)other;
            return r.Equals(rhs.r) && g.Equals(rhs.g) && b.Equals(rhs.b) && a.Equals(rhs.a);
        }

        public static Color operator +(Color a, Color b) { return new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a); }
        public static Color operator -(Color a, Color b) { return new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a); }
        public static Color operator *(Color a, Color b) { return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a); }
        public static Color operator *(Color a, float b) { return new Color(a.r * b, a.g * b, a.b * b, a.a * b); }
        public static Color operator *(float b, Color a) { return new Color(a.r * b, a.g * b, a.b * b, a.a * b); }
        public static Color operator /(Color a, float b) { return new Color(a.r / b, a.g / b, a.b / b, a.a / b); }

        public static bool operator ==(Color lhs, Color rhs)
        {
            return ((Vector4)lhs == (Vector4)rhs);
        }
        public static bool operator !=(Color lhs, Color rhs)
        {
            return ((Vector4)lhs != (Vector4)rhs);
        }

        public static Color Lerp(Color a, Color b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color(
                a.r + (b.r - a.r) * t,
                a.g + (b.g - a.g) * t,
                a.b + (b.b - a.b) * t,
                a.a + (b.a - a.a) * t
            );
        }

        internal Color RGBMultiplied(float multiplier) { return new Color(r * multiplier, g * multiplier, b * multiplier, a); }
        internal Color AlphaMultiplied(float multiplier) { return new Color(r, g, b, a * multiplier); }
        internal Color RGBMultiplied(Color multiplier) { return new Color(r * multiplier.r, g * multiplier.g, b * multiplier.b, a); }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Color32
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public Color32(byte r, byte g, byte b, byte a)
        {
            this.r = r; this.g = g; this.b = b; this.a = a;
        }

        public static implicit operator Color32(Color c)
        {
            return new Color32((byte)(Mathf.Clamp01(c.r) * 255), (byte)(Mathf.Clamp01(c.g) * 255), (byte)(Mathf.Clamp01(c.b) * 255), (byte)(Mathf.Clamp01(c.a) * 255));
        }

        public static implicit operator Color(Color32 c)
        {
            return new Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
        }

        override public string ToString()
        {
            return String.Format("RGBA({0}, {1}, {2}, {3})", r, g, b, a);
        }
        public string ToString(string format)
        {
            return String.Format("RGBA({0}, {1}, {2}, {3})", r.ToString(format), g.ToString(format), b.ToString(format), a.ToString(format));
        }

        public static Color32 Lerp(Color32 a, Color32 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color32(
                (byte)(a.r + (b.r - a.r) * t),
                (byte)(a.g + (b.g - a.g) * t),
                (byte)(a.b + (b.b - a.b) * t),
                (byte)(a.a + (b.a - a.a) * t)
            );
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public const float kEpsilon = 0.000001F;

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    case 3: return w;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    case 3: w = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index!");
                }
            }
        }
        
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        
        public void Set(float new_x, float new_y, float new_z, float new_w) { x = new_x; y = new_y; z = new_z; w = new_w; }
        
        public static Quaternion identity { get { return new Quaternion(0F, 0F, 0F, 1F); } }

        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(
                    lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                    lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
                    lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
                    lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }
        
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float x = rotation.x * 2F;
            float y = rotation.y * 2F;
            float z = rotation.z * 2F;
            float xx = rotation.x * x;
            float yy = rotation.y * y;
            float zz = rotation.z * z;
            float xy = rotation.x * y;
            float xz = rotation.x * z;
            float yz = rotation.y * z;
            float wx = rotation.w * x;
            float wy = rotation.w * y;
            float wz = rotation.w * z;

            Vector3 res;
            res.x = (1F - (yy + zz)) * point.x + (xy - wz) * point.y + (xz + wy) * point.z;
            res.y = (xy + wz) * point.x + (1F - (xx + zz)) * point.y + (yz - wx) * point.z;
            res.z = (xz - wy) * point.x + (yz + wx) * point.y + (1F - (xx + yy)) * point.z;
            return res;
        }

        public static bool operator ==(Quaternion lhs, Quaternion rhs)
        {
            return Dot(lhs, rhs) > 1.0f - kEpsilon;
        }
        
        public static bool operator !=(Quaternion lhs, Quaternion rhs)
        {
            return Dot(lhs, rhs) <= 1.0f - kEpsilon;
        }
        
        public static float Dot(Quaternion a, Quaternion b) { return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w; }

        public static float Magnitude(Quaternion q)
        {
            return Mathf.Sqrt(Dot(q, q));
        }
        
        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            angle = Mathf.DegreeToRadius(angle);

            Quaternion q;
            float mag = Vector3.Magnitude(axis);
            if (mag > 0.000001F)
            {
                float halfAngle = angle * 0.5F;

                q.w = Mathf.Cos(halfAngle);

                float s = Mathf.Sin(halfAngle) / mag;
                q.x = s * axis.x;
                q.y = s * axis.y;
                q.z = s * axis.z;
                return q;
            }
            else
            {
                return identity;
            }
        }

        public static Quaternion Normalize(Quaternion q)
        {
            Quaternion ret;
            float scalar = Magnitude(q);
            ret.x = q.x / scalar;
            ret.y = q.y / scalar;
            ret.z = q.z / scalar;
            ret.w = q.w / scalar;
            return ret;
        }

        public static Quaternion NormalizeSafe(Quaternion q)
        {
            float mag = Magnitude(q);
            if (mag < Vector3.kEpsilon)
                return identity;
            else
                return Normalize(q);
        }

        public static void MatrixToQuaternion(Matrix4x4 kRot, ref Quaternion q)
        {
            float fTrace = kRot.m00 + kRot.m11 + kRot.m22;
            float fRoot;

            if (fTrace > 0.0f)
            {
                // |w| > 1/2, may as well choose w > 1/2
                fRoot = Mathf.Sqrt(fTrace + 1.0f);  // 2w
                q.w = 0.5f * fRoot;
                fRoot = 0.5f / fRoot;  // 1/(4w)
                q.x = (kRot.m21 - kRot.m12) * fRoot;
                q.y = (kRot.m02 - kRot.m20) * fRoot;
                q.z = (kRot.m10 - kRot.m01) * fRoot;
            }
            else
            {
                // |w| <= 1/2
                int[] s_iNext = { 1, 2, 0 };

                int i = 0;
                if (kRot.m11 > kRot.m00)
                    i = 1;
                if (kRot.m22 > kRot[i,i])
                    i = 2;
                int j = s_iNext[i];
                int k = s_iNext[j];

                fRoot = Mathf.Sqrt(kRot[i, i] - kRot[i, j] - kRot[k, k] + 1.0f);
                
                q[i] = 0.5f * fRoot;
                fRoot = 0.5f / fRoot;
                q.w = (kRot[k,j] - kRot[j,k]) * fRoot;
                q[j] = (kRot[j,i] + kRot[i,j]) * fRoot;
                q[k] = (kRot[k,j] + kRot[i,k]) * fRoot;
            }
            q = Normalize(q);
        }

        public static Quaternion FromToQuaternion(Vector3 from, Vector3 to)
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.SetFromToRotation(from, to);

            Quaternion q = Quaternion.identity;
            MatrixToQuaternion(m, ref q);
            return q;
        }

        public static Quaternion FromToRotation(Vector3 lhs, Vector3 rhs)
        {
            float lhsMag = Vector3.Magnitude(lhs);
            float rhsMag = Vector3.Magnitude(rhs);
            if (lhsMag < Vector3.kEpsilon || rhsMag < Vector3.kEpsilon)
                return Quaternion.identity;
            else
                return FromToQuaternion(lhs / lhsMag, rhs / rhsMag);
        }

        public void SetFromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            this = FromToRotation(fromDirection, toDirection);
        }

        public static bool CompareApproximately(float f0, float f1, float epsilon = 0.000001F)
        {
            float dist = f0 - f1;
            dist = Math.Abs(dist);
            return dist < epsilon;
        }

        public static bool LookRotationToMatrix(Vector3 viewVec, Vector3 upVec, ref Matrix4x4 m)
        {
            Vector3 z = viewVec;
            // compute u0
            float mag = Vector3.Magnitude(z);
            if (mag < Vector3.kEpsilon)
            {
                m = Matrix4x4.identity;
                return false;
            }
            z /= mag;

            Vector3 x = Vector3.Cross(upVec, z);
            mag = Vector3.Magnitude(x);
            if (mag < Vector3.kEpsilon)
            {
                m = Matrix4x4.identity;
                return false;
            }
            x /= mag;

            Vector3 y = Vector3.Cross(z, x);
            if (!CompareApproximately(Vector3.SqrMagnitude(y), 1.0F))
                return false;
            
            m.SetOrthoNormalBasis(x, y, z);
            return true;
        }

        public static bool LookRotationToQuaternion(Vector3 viewVec, Vector3 upVec, ref Quaternion res)
        {
            Matrix4x4 m = Matrix4x4.identity;
            if(!LookRotationToMatrix(viewVec, upVec, ref m))
            {
                return false;
            }
            MatrixToQuaternion(m, ref res);
            return true;
        }

        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards)
        {
            Quaternion q = Quaternion.identity;
            if (!LookRotationToQuaternion(forward, upwards, ref q))
            {
                float mag = Vector3.Magnitude(forward);
                if (mag > Vector3.kEpsilon)
                {
                    Matrix4x4 m = Matrix4x4.identity;
                    m.SetFromToRotation(Vector3.forward, forward / mag);
                }
            }
            return q;
        }

        public static Quaternion LookRotation(Vector3 forward)
        {
            return LookRotation(forward, Vector3.up);
        }

        public void SetLookRotation(Vector3 view)
        {
            SetLookRotation(view, Vector3.up);
        }

        public void SetLookRotation(Vector3 view, Vector3 up) { this = LookRotation(view, up); }
        
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, float t)
        {
            return UnclampedSlerp(q1, q2, Mathf.Clamp01(t));
        }

        public static Quaternion Lerp(Quaternion q1, Quaternion q2, float t)
        {
            Quaternion tmpQuat = Quaternion.identity;
            // if (dot < 0), q1 and q2 are more than 360 deg apart.
            // The problem is that quaternions are 720deg of freedom.
            // so we - all components when lerping
            if (Dot(q1, q2) < 0.0F)
            {
                tmpQuat.Set(q1.x + t * (-q2.x - q1.x),
                            q1.y + t * (-q2.y - q1.y),
                            q1.z + t * (-q2.z - q1.z),
                            q1.w + t * (-q2.w - q1.w));
            }
            else
            {
                tmpQuat.Set(q1.x + t * (q2.x - q1.x),
                            q1.y + t * (q2.y - q1.y),
                            q1.z + t * (q2.z - q1.z),
                            q1.w + t * (q2.w - q1.w));
            }
            return Normalize(tmpQuat);
        }

        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
        {
            float angle = Quaternion.Angle(from, to);
            if (angle == 0.0f)
                return to;
            float slerpValue = Mathf.Min(1.0f, maxDegreesDelta / angle);
            return UnclampedSlerp(from, to, slerpValue);
        }

        private static Quaternion UnclampedSlerp(Quaternion q1, Quaternion q2, float t)
        {
            float dot = Dot(q1, q2);

            // dot = cos(theta)
            // if (dot < 0), q1 and q2 are more than 90 degrees apart,
            // so we can invert one to reduce spinning
            Quaternion tmpQuat = Quaternion.identity;
            if (dot < 0.0f)
            {
                dot = -dot;
                tmpQuat.Set(-q2.x,
                             -q2.y,
                             -q2.z,
                             -q2.w);
            }
            else
                tmpQuat = q2;


            if (dot < 0.95f)
            {
                float angle = Mathf.Acos(dot);
                float sinadiv, sinat, sinaomt;
                sinadiv = 1.0f / Mathf.Sin(angle);
                sinat = Mathf.Sin(angle * t);
                sinaomt = Mathf.Sin(angle * (1.0f - t));
                tmpQuat.Set((q1.x * sinaomt + tmpQuat.x * sinat) * sinadiv,
                         (q1.y * sinaomt + tmpQuat.y * sinat) * sinadiv,
                         (q1.z * sinaomt + tmpQuat.z * sinat) * sinadiv,
                         (q1.w * sinaomt + tmpQuat.w * sinat) * sinadiv);
                return tmpQuat;

            }

            else
            {
                return Lerp(q1, tmpQuat, t);
            }
        }

        public static Quaternion Conjugate(Quaternion q)
        {
            Quaternion ret;
            ret.x = -q.x;
            ret.y = -q.y;
            ret.z = -q.z;
            ret.w = q.w;
            return ret;
        }

        public static Quaternion Inverse(Quaternion q)
        {
            return Conjugate(q);
        }

        override public string ToString()
        {
            return String.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", x, y, z, w);
        }
        public string ToString(string format)
        {
            return String.Format("({0}, {1}, {2}, {3})", x.ToString(format), y.ToString(format), z.ToString(format), w.ToString(format));
        }

        static public float Angle(Quaternion a, Quaternion b)
        {
            float dot = Dot(a, b);
            return Mathf.Acos(Mathf.Min(Mathf.Abs(dot), 1.0F)) * 2.0F * Mathf.Rad2Deg;
        }

        public Vector3 eulerAngles { get { return Internal_ToEulerRad(this) * Mathf.Rad2Deg; } set { this = Internal_FromEulerRad(value * Mathf.Deg2Rad); } }
        
        static public Quaternion Euler(float x, float y, float z) { return Internal_FromEulerRad(new Vector3(x, y, z) * Mathf.Deg2Rad); }
        
        static public Quaternion Euler(Vector3 euler) { return Internal_FromEulerRad(euler * Mathf.Deg2Rad); }
        
        public static Matrix4x4 QuaternionToMatrix(Quaternion q)
        {
            Matrix4x4 m = Matrix4x4.identity;
            float x = q.x * 2.0F;
            float y = q.y * 2.0F;
            float z = q.z * 2.0F;
            float xx = q.x * x;
            float yy = q.y * y;
            float zz = q.z * z;
            float xy = q.x * y;
            float xz = q.x * z;
            float yz = q.y * z;
            float wx = q.w * x;
            float wy = q.w * y;
            float wz = q.w * z;

            // Calculate 3x3 matrix from orthonormal basis
            m[0,0] = 1.0f - (yy + zz);
            m[0,1] = xy + wz;
            m[0,2] = xz - wy;
            m[0,3] = 0.0f;

            m[1,0] = xy - wz;
            m[1,1] = 1.0f - (xx + zz);
            m[1,2] = yz + wx;
            m[1,3] = 0.0f;

            m[2,0] = xz + wy;
            m[2,1] = yz - wx;
            m[2,2] = 1.0f - (xx + yy);
            m[2,3] = 0.0f;

            m[3,0] = xz + wy;
            m[3,1] = yz - wx;
            m[3,2] = 1.0f - (xx + yy);
            m[3,3] = 1.0f;

            return m;
        }

        public static void MakePositive(Vector3 euler)
        {
            const float negativeFlip = -0.0001F;
            const float positiveFlip = (float)(Math.PI * 2.0F) - 0.0001F;

            if (euler.x < negativeFlip)
                euler.x += (float)(2.0 * Math.PI);
            else if (euler.x > positiveFlip)
                euler.x -= (float)(2.0 * Math.PI);

            if (euler.y < negativeFlip)
                euler.y += (float)(2.0 * Math.PI);
            else if (euler.y > positiveFlip)
                euler.y -= (float)(2.0 * Math.PI);

            if (euler.z < negativeFlip)
                euler.z += (float)(2.0 * Math.PI);
            else if (euler.z > positiveFlip)
                euler.z -= (float)(2.0 * Math.PI);
        }

        public static bool MatrixToEuler(Matrix4x4 matrix, ref Vector3 v)
        {
            // from http://www.geometrictools.com/Documentation/EulerAngles.pdf
            // YXZ order
            if (matrix[1,2] < 0.999F) // some fudge for imprecision
            {
                if (matrix[1,2] > -0.999F) // some fudge for imprecision
                {
                    v.x = Mathf.Asin(-matrix[1,2]);
                    v.y = Mathf.Atan2(matrix[0,2], matrix[2,2]);
                    v.z = Mathf.Atan2(matrix[1,0], matrix[1,1]);
                    MakePositive(v);
                    return true;
                }
                else
                {
                    // WARNING.  Not unique.  YA - ZA = atan2(r01,r00)
                    v.x = Mathf.PI * 0.5F;
                    v.y = Mathf.Atan2(matrix[0,1], matrix[0,0]);
                    v.z = 0.0F;
                    MakePositive(v);

                    return false;
                }
            }
            else
            {
                // WARNING.  Not unique.  YA + ZA = atan2(-r01,r00)
                v.x = -Mathf.PI * 0.5F;
                v.y = Mathf.Atan2(-matrix[0,1], matrix[0,0]);
                v.z = 0.0F;
                MakePositive(v);
                return false;
            }
        }
        public static Vector3 QuaternionToEuler(Quaternion quat)
        {
            Matrix4x4 m = Matrix4x4.identity;
            Vector3 rot = Vector3.zero;
            m = QuaternionToMatrix(quat);
            MatrixToEuler(m, ref rot);
            return rot;
        }
        private static Vector3 Internal_ToEulerRad(Quaternion rotation)
        {
            Quaternion outRotation = NormalizeSafe(rotation);
            return QuaternionToEuler(outRotation);
        }

        public static Quaternion EulerToQuaternion(Vector3 someEulerAngles)
        {
            float cX = (float)(Math.Cos(someEulerAngles.x / 2.0f));
            float sX = (float)(Math.Sin(someEulerAngles.x / 2.0f));

            float cY = (float)(Math.Cos(someEulerAngles.y / 2.0f));
            float sY = (float)(Math.Sin(someEulerAngles.y / 2.0f));

            float cZ = (float)(Math.Cos(someEulerAngles.z / 2.0f));
            float sZ = (float)(Math.Sin(someEulerAngles.z / 2.0f));

            Quaternion qX = new Quaternion(sX, 0.0F, 0.0F, cX);
            Quaternion qY = new Quaternion(0.0F, sY, 0.0F, cY);
            Quaternion qZ = new Quaternion(0.0F, 0.0F, sZ, cZ);

            Quaternion q = (qY * qX) * qZ;
            return q;
        }

        private static Quaternion Internal_FromEulerRad(Vector3 euler)
        {
            return EulerToQuaternion(euler);
        }

        private static void Internal_ToAxisAngleRad(Quaternion q, out Vector3 axis, out float targetAngle)
        {
            targetAngle = (float)(2.0f * Math.Acos(q.w));
            if (CompareApproximately(targetAngle, 0.0F))
            {
                axis = Vector3.right;
                return;
            }

            axis = Vector3.zero;
            float div = (float)(1.0f / Math.Sqrt(1.0f - q.w*q.w));
            axis.Set(q.x * div, q.y * div, q.z * div);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2) ^ (w.GetHashCode() >> 1);
        }

        public override bool Equals(object other)
        {
            if (!(other is Quaternion)) return false;

            Quaternion rhs = (Quaternion)other;
            return x.Equals(rhs.x) && y.Equals(rhs.y) && z.Equals(rhs.z) && w.Equals(rhs.w);
        }

        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            Quaternion q;
            double t0 = Math.Cos(yaw * 0.5f);
            double t1 = Math.Sin(yaw * 0.5f);
            double t2 = Math.Cos(roll * 0.5f);
            double t3 = Math.Sin(roll * 0.5f);
            double t4 = Math.Cos(pitch * 0.5f);
            double t5 = Math.Sin(pitch * 0.5f);

            q.w = (float)(t0 * t2 * t4 + t1 * t3 * t5);
            q.x = (float)(t0 * t3 * t4 - t1 * t2 * t5);
            q.y = (float)(t0 * t2 * t5 + t1 * t3 * t4);
            q.z = (float)(t1 * t2 * t4 - t0 * t3 * t5);
            return q;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        private float m_XMin, m_YMin, m_Width, m_Height;

        public Rect(float left, float top, float width, float height)
        {
            m_XMin = left;
            m_YMin = top;
            m_Width = width;
            m_Height = height;
        }

        public Rect(Rect source)
        {
            m_XMin = source.m_XMin;
            m_YMin = source.m_YMin;
            m_Width = source.m_Width;
            m_Height = source.m_Height;
        }

        static public Rect MinMaxRect(float left, float top, float right, float bottom)
        {
            return new Rect(left, top, right - left, bottom - top);
        }


        public void Set(float left, float top, float width, float height)
        {
            m_XMin = left;
            m_YMin = top;
            m_Width = width;
            m_Height = height;
        }
        
        public float x { get { return m_XMin; } set { m_XMin = value; } }

        public float y { get { return m_YMin; } set { m_YMin = value; } }

        public Vector2 center { get { return new Vector2(x + m_Width / 2f, y + m_Height / 2f); } set { m_XMin = value.x - m_Width / 2f; m_YMin = value.y - m_Height / 2f; } }

        public float width { get { return m_Width; } set { m_Width = value; } }
        
        public float height { get { return m_Height; } set { m_Height = value; } }

        public float xMin { get { return m_XMin; } set { float oldxmax = xMax; m_XMin = value; m_Width = oldxmax - m_XMin; } }
        public float yMin { get { return m_YMin; } set { float oldymax = yMax; m_YMin = value; m_Height = oldymax - m_YMin; } }
        public float xMax { get { return m_Width + m_XMin; } set { m_Width = value - m_XMin; } }
        public float yMax { get { return m_Height + m_YMin; } set { m_Height = value - m_YMin; } }
        
        override public string ToString()
        {
            return String.Format("(x:{0:F2}, y:{1:F2}, width:{2:F2}, height:{3:F2})", x, y, width, height);
        }
        public string ToString(string format)
        {
            return String.Format("(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format), y.ToString(format), width.ToString(format), height.ToString(format));
        }

        public bool Contains(Vector2 point)
        {
            return (point.x >= xMin) && (point.x < xMax) && (point.y >= yMin) && (point.y < yMax);
        }
        public bool Contains(Vector3 point)
        {
            return (point.x >= xMin) && (point.x < xMax) && (point.y >= yMin) && (point.y < yMax);
        }
        public bool Contains(Vector3 point, bool allowInverse)
        {
            if (!allowInverse)
            {
                return Contains(point);
            }
            bool xAxis = false;
            if (width < 0f && (point.x <= xMin) && (point.x > xMax) || width >= 0f && (point.x >= xMin) && (point.x < xMax))
                xAxis = true;
            if (xAxis && (height < 0f && (point.y <= yMin) && (point.y > yMax) || height >= 0f && (point.y >= yMin) && (point.y < yMax)))
                return true;
            return false;
        }

        private static Rect OrderMinMax(Rect rect)
        {
            if (rect.xMin > rect.xMax)
            {
                float temp = rect.xMin;
                rect.xMin = rect.xMax;
                rect.xMax = temp;
            }
            if (rect.yMin > rect.yMax)
            {
                float temp = rect.yMin;
                rect.yMin = rect.yMax;
                rect.yMax = temp;
            }
            return rect;
        }

        public bool Overlaps(Rect other)
        {
            return (other.xMax > xMin &&
                    other.xMin < xMax &&
                    other.yMax > yMin &&
                    other.yMin < yMax);
        }

        public bool Overlaps(Rect other, bool allowInverse)
        {
            Rect self = this;
            if (allowInverse)
            {
                self = OrderMinMax(self);
                other = OrderMinMax(other);
            }
            return self.Overlaps(other);
        }
        
        public static bool operator !=(Rect lhs, Rect rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y || lhs.width != rhs.width || lhs.height != rhs.height;
        }
        
        public static bool operator ==(Rect lhs, Rect rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (width.GetHashCode() << 2) ^ (y.GetHashCode() >> 2) ^ (height.GetHashCode() >> 1);
        }

        public override bool Equals(object other)
        {
            if (!(other is Rect)) return false;

            Rect rhs = (Rect)other;
            return x.Equals(rhs.x) && y.Equals(rhs.y) && width.Equals(rhs.width) && height.Equals(rhs.height);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4x4
    {
        public float m00;
        public float m10;
        public float m20;
        public float m30;

        public float m01;
        public float m11;
        public float m21;
        public float m31;

        public float m02;
        public float m12;
        public float m22;
        public float m32;

        public float m03;
        public float m13;
        public float m23;
        public float m33;
        
        public float this[int row, int column]
        {
            get
            {
                return this[row + column * 4];
            }

            set
            {
                this[row + column * 4] = value;
            }
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return m00;
                    case 1: return m10;
                    case 2: return m20;
                    case 3: return m30;
                    case 4: return m01;
                    case 5: return m11;
                    case 6: return m21;
                    case 7: return m31;
                    case 8: return m02;
                    case 9: return m12;
                    case 10: return m22;
                    case 11: return m32;
                    case 12: return m03;
                    case 13: return m13;
                    case 14: return m23;
                    case 15: return m33;
                    default:
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: m00 = value; break;
                    case 1: m10 = value; break;
                    case 2: m20 = value; break;
                    case 3: m30 = value; break;
                    case 4: m01 = value; break;
                    case 5: m11 = value; break;
                    case 6: m21 = value; break;
                    case 7: m31 = value; break;
                    case 8: m02 = value; break;
                    case 9: m12 = value; break;
                    case 10: m22 = value; break;
                    case 11: m32 = value; break;
                    case 12: m03 = value; break;
                    case 13: m13 = value; break;
                    case 14: m23 = value; break;
                    case 15: m33 = value; break;

                    default:
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }
        }

        public override int GetHashCode()
        {
            return GetColumn(0).GetHashCode() ^ (GetColumn(1).GetHashCode() << 2) ^ (GetColumn(2).GetHashCode() >> 2) ^ (GetColumn(3).GetHashCode() >> 1);
        }

        public override bool Equals(object other)
        {
            if (!(other is Matrix4x4)) return false;

            Matrix4x4 rhs = (Matrix4x4)other;
            return GetColumn(0).Equals(rhs.GetColumn(0))
                && GetColumn(1).Equals(rhs.GetColumn(1))
                && GetColumn(2).Equals(rhs.GetColumn(2))
                && GetColumn(3).Equals(rhs.GetColumn(3));
        }

        static public Matrix4x4 operator *(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            Matrix4x4 res = new Matrix4x4();
            res.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
            res.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
            res.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
            res.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;

            res.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
            res.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
            res.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
            res.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;

            res.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
            res.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
            res.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
            res.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;

            res.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
            res.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
            res.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
            res.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;

            return res;
        }
        
        static public Vector4 operator *(Matrix4x4 lhs, Vector4 v)
        {
            Vector4 res;
            res.x = lhs.m00 * v.x + lhs.m01 * v.y + lhs.m02 * v.z + lhs.m03 * v.w;
            res.y = lhs.m10 * v.x + lhs.m11 * v.y + lhs.m12 * v.z + lhs.m13 * v.w;
            res.z = lhs.m20 * v.x + lhs.m21 * v.y + lhs.m22 * v.z + lhs.m23 * v.w;
            res.w = lhs.m30 * v.x + lhs.m31 * v.y + lhs.m32 * v.z + lhs.m33 * v.w;
            return res;
        }

        public static bool operator ==(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return lhs.GetColumn(0) == rhs.GetColumn(0)
                && lhs.GetColumn(1) == rhs.GetColumn(1)
                && lhs.GetColumn(2) == rhs.GetColumn(2)
                && lhs.GetColumn(3) == rhs.GetColumn(3);
        }
        public static bool operator !=(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return !(lhs == rhs);
        }

        public bool isIdentity
        {
            get { return default(bool); }
        }

        public Vector4 GetColumn(int i) { return new Vector4(this[0, i], this[1, i], this[2, i], this[3, i]); }
        public Vector4 GetRow(int i) { return new Vector4(this[i, 0], this[i, 1], this[i, 2], this[i, 3]); }
        public void SetColumn(int i, Vector4 v) { this[0, i] = v.x; this[1, i] = v.y; this[2, i] = v.z; this[3, i] = v.w; }
        public void SetRow(int i, Vector4 v) { this[i, 0] = v.x; this[i, 1] = v.y; this[i, 2] = v.z; this[i, 3] = v.w; }
        
        public Vector3 MultiplyPoint(Vector3 v)
        {
            Vector3 res;
            float w;
            res.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z + this.m03;
            res.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z + this.m13;
            res.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z + this.m23;
            w = this.m30 * v.x + this.m31 * v.y + this.m32 * v.z + this.m33;

            w = 1F / w;
            res.x *= w;
            res.y *= w;
            res.z *= w;
            return res;
        }

        public Vector3 MultiplyPoint3x4(Vector3 v)
        {
            Vector3 res;
            res.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z + this.m03;
            res.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z + this.m13;
            res.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z + this.m23;
            return res;
        }

        public Vector3 MultiplyVector(Vector3 v)
        {
            Vector3 res;
            res.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z;
            res.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z;
            res.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z;
            return res;
        }

        public static Matrix4x4 Scale(Vector3 v)
        {
            Matrix4x4 m = new Matrix4x4();
            m.m00 = v.x; m.m01 = 0F; m.m02 = 0F; m.m03 = 0F;
            m.m10 = 0F; m.m11 = v.y; m.m12 = 0F; m.m13 = 0F;
            m.m20 = 0F; m.m21 = 0F; m.m22 = v.z; m.m23 = 0F;
            m.m30 = 0F; m.m31 = 0F; m.m32 = 0F; m.m33 = 1F;
            return m;
        }

        public static Matrix4x4 GetRotMatrixNormVec(Vector3 inVec, float radians)
        {
            Matrix4x4 M;
            float s, c;
            float vx, vy, vz, xx, yy, zz, xy, yz, zx, xs, ys, zs, one_c;

            s = Mathf.Sin(radians);
            c = Mathf.Cos(radians);

            vx = inVec[0];
            vy = inVec[1];
            vz = inVec[2];

            xx = vx * vx;
            yy = vy * vy;
            zz = vz * vz;
            xy = vx * vy;
            yz = vy * vz;
            zx = vz * vx;
            xs = vx * s;
            ys = vy * s;
            zs = vz * s;
            one_c = 1.0F - c;

            M.m00 = (one_c * xx) + c;
            M.m10 = (one_c * xy) - zs;
            M.m20 = (one_c * zx) + ys;
            M.m30 = 0.0f;

            M.m01 = (one_c * xy) + zs;
            M.m11 = (one_c * yy) + c;
            M.m21 = (one_c * yz) - xs;
            M.m31 = 0.0f;

            M.m02 = (one_c * zx) - ys;
            M.m12 = (one_c * yz) + xs;
            M.m22 = (one_c * zz) + c;
            M.m32 = 0.0f;

            M.m03 = 0.0f;
            M.m13 = 0.0f;
            M.m23 = 0.0f;
            M.m33 = 1.0f;

            return M;
        }

        public Matrix4x4 SetOrthoNormalBasis(Vector3 inX, Vector3 inY, Vector3 inZ)
        {
            m00 = inX[0]; m01 = inY[0]; m02 = inZ[0];
            m10 = inX[1]; m11 = inY[1]; m12 = inZ[1];
            m20 = inX[2]; m21 = inY[2]; m22 = inZ[2];
            return this;
        }

        public static Matrix4x4 FromToRotation(Vector3 from, Vector3 to)
        {
            Matrix4x4 mtx = Matrix4x4.identity;
            Vector3 v;
            float e, h;

            v = Vector3.Cross(from, to);
            e = Vector3.Dot(from, to);
            if (e > 1.0f - Vector3.kEpsilon)
            {
                mtx = Matrix4x4.identity;
            }
            else if (e < -1.0f - Vector3.kEpsilon)
            {
                Vector3 up, left;
                float invlen;
                float fxx, fyy, fzz, fxy, fxz, fyz;
                float uxx, uyy, uzz, uxy, uxz, uyz;
                float lxx, lyy, lzz, lxy, lxz, lyz;
                
                left.x = 0.0f; left.y = from[2]; left.z = -from[1];
                if (Vector3.Dot(left, left) < Vector3.kEpsilon)
                {
                    left.x = -from[2]; left.y = 0.0f; left.z = from[0];
                }

                /* normalize "left" */
                invlen = 1.0f / Mathf.Sqrt((float)Vector3.Dot(left, left));
                left[0] *= invlen;
                left[1] *= invlen;
                left[2] *= invlen;
                up = Vector3.Cross(left, from);

                /* now we have a coordinate system, i.e., a basis;    */
                /* M=(from, up, left), and we want to rotate to:      */
                /* N=(-from, up, -left). This is done with the matrix:*/
                /* N*M^T where M^T is the transpose of M              */
                fxx = -from[0] * from[0]; fyy = -from[1] * from[1]; fzz = -from[2] * from[2];
                fxy = -from[0] * from[1]; fxz = -from[0] * from[2]; fyz = -from[1] * from[2];

                uxx = up[0] * up[0]; uyy = up[1] * up[1]; uzz = up[2] * up[2];
                uxy = up[0] * up[1]; uxz = up[0] * up[2]; uyz = up[1] * up[2];

                lxx = -left[0] * left[0]; lyy = -left[1] * left[1]; lzz = -left[2] * left[2];
                lxy = -left[0] * left[1]; lxz = -left[0] * left[2]; lyz = -left[1] * left[2];
                /* symmetric matrix */
                mtx.m00 = fxx + uxx + lxx; mtx.m01 = fxy + uxy + lxy; mtx.m02 = fxz + uxz + lxz;
                mtx.m10 = mtx.m01; mtx.m11 = fyy + uyy + lyy; mtx.m12 = fyz + uyz + lyz;
                mtx.m20 = mtx.m02; mtx.m21 = mtx.m12; mtx.m22 = fzz + uzz + lzz;
            }
            else
            {
                float hvx, hvz, hvxy, hvxz, hvyz;
                h = (1.0f - e) / Vector3.Dot(v, v);
                hvx = h * v[0];
                hvz = h * v[2];
                hvxy = hvx * v[1];
                hvxz = hvx * v[2];
                hvyz = hvz * v[1];
                mtx.m00 = e + hvx * v[0]; mtx.m01 = hvxy - v[2]; mtx.m02 = hvxz + v[1];
                mtx.m10 = hvxy + v[2]; mtx.m11 = e + h * v[1] * v[1]; mtx.m12 = hvyz - v[0];
                mtx.m20 = hvxz - v[1]; mtx.m21 = hvyz + v[0]; mtx.m22 = e + hvz * v[2];
            }
            return mtx;
        }

        public Matrix4x4 SetFromToRotation(Vector3 from, Vector3 to)
        {
            Matrix4x4 mtx = FromToRotation(from, to);
            m00 = mtx.m00; m01 = mtx.m01; m02 = mtx.m02; m03 = mtx.m03;
            m10 = mtx.m10; m11 = mtx.m11; m12 = mtx.m12; m13 = mtx.m13;
            m20 = mtx.m20; m21 = mtx.m21; m22 = mtx.m22; m23 = mtx.m23;
            m30 = mtx.m30; m31 = mtx.m31; m32 = mtx.m32; m33 = mtx.m33;
            return this;
        }

        public static Matrix4x4 SetAxisAngle(Vector3 rotationAxis, float radians)
        {
            return GetRotMatrixNormVec(rotationAxis, radians);
        }

        public static Matrix4x4 zero
        {
            get
            {
                Matrix4x4 m = new Matrix4x4();
                m.m00 = 0F; m.m01 = 0F; m.m02 = 0F; m.m03 = 0F;
                m.m10 = 0F; m.m11 = 0F; m.m12 = 0F; m.m13 = 0F;
                m.m20 = 0F; m.m21 = 0F; m.m22 = 0F; m.m23 = 0F;
                m.m30 = 0F; m.m31 = 0F; m.m32 = 0F; m.m33 = 0F;
                return m;
            }
        }

        public static Matrix4x4 identity
        {
            get
            {
                Matrix4x4 m = new Matrix4x4();
                m.m00 = 1F; m.m01 = 0F; m.m02 = 0F; m.m03 = 0F;
                m.m10 = 0F; m.m11 = 1F; m.m12 = 0F; m.m13 = 0F;
                m.m20 = 0F; m.m21 = 0F; m.m22 = 1F; m.m23 = 0F;
                m.m30 = 0F; m.m31 = 0F; m.m32 = 0F; m.m33 = 1F;
                return m;
            }
        }

        override public string ToString()
        {
            return String.Format("{0:F5}\t{1:F5}\t{2:F5}\t{3:F5}\n{4:F5}\t{5:F5}\t{6:F5}\t{7:F5}\n{8:F5}\t{9:F5}\t{10:F5}\t{11:F5}\n{12:F5}\t{13:F5}\t{14:F5}\t{15:F5}\n", m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33);
        }
        public string ToString(string format)
        {
            return String.Format("{0}\t{1}\t{2}\t{3}\n{4}\t{5}\t{6}\t{7}\n{8}\t{9}\t{10}\t{11}\n{12}\t{13}\t{14}\t{15}\n",
                m00.ToString(format), m01.ToString(format), m02.ToString(format), m03.ToString(format),
                m10.ToString(format), m11.ToString(format), m12.ToString(format), m13.ToString(format),
                m20.ToString(format), m21.ToString(format), m22.ToString(format), m23.ToString(format),
                m30.ToString(format), m31.ToString(format), m32.ToString(format), m33.ToString(format));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Bounds
    {
        private Vector3 m_Center;
        private Vector3 m_Extents;
        
        public Bounds(Vector3 center, Vector3 size)
        {
            m_Center = center;
            m_Extents = size * 0.5F;
        }

        public override int GetHashCode()
        {
            return center.GetHashCode() ^ (extents.GetHashCode() << 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Bounds)) return false;

            Bounds rhs = (Bounds)other;
            return center.Equals(rhs.center) && extents.Equals(rhs.extents);
        }
        
        public Vector3 center { get { return m_Center; } set { m_Center = value; } }
        public Vector3 size { get { return m_Extents * 2.0F; } set { m_Extents = value * 0.5F; } }
        public Vector3 extents { get { return m_Extents; } set { m_Extents = value; } }
        public Vector3 min { get { return center - extents; } set { SetMinMax(value, max); } }
        public Vector3 max { get { return center + extents; } set { SetMinMax(min, value); } }
        
        public static bool operator ==(Bounds lhs, Bounds rhs)
        {
            return (lhs.center == rhs.center && lhs.extents == rhs.extents);
        }
        public static bool operator !=(Bounds lhs, Bounds rhs)
        {
            return !(lhs == rhs);
        }
        
        public void SetMinMax(Vector3 min, Vector3 max)
        {
            extents = (max - min) * 0.5F;
            center = min + extents;
        }

        public void Encapsulate(Vector3 point)
        {
            SetMinMax(Vector3.Min(min, point), Vector3.Max(max, point));
        }
        
        public void Encapsulate(Bounds bounds)
        {
            Encapsulate(bounds.center - bounds.extents);
            Encapsulate(bounds.center + bounds.extents);
        }

        public void Expand(float amount)
        {
            amount *= .5f;
            extents += new Vector3(amount, amount, amount);
        }

        public void Expand(Vector3 amount)
        {
            extents += amount * .5f;
        }

        public bool Intersects(Bounds bounds)
        {
            return (min.x <= bounds.max.x) && (max.x >= bounds.min.x) &&
                   (min.y <= bounds.max.y) && (max.y >= bounds.min.y) &&
                   (min.z <= bounds.max.z) && (max.z >= bounds.min.z);
        }
        
        override public string ToString()
        {
            return String.Format("Center: {0}, Extents: {1}", m_Center, m_Extents);
        }
        public string ToString(string format)
        {
            return String.Format("Center: {0}, Extents: {1}", m_Center.ToString(format), m_Extents.ToString(format));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4
    {
        public const float kEpsilon = 0.00001F;


        public float x;
        public float y;
        public float z;
        public float w;

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    case 3: return w;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector4 index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    case 3: w = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector4 index!");
                }
            }
        }

        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public Vector4(float x, float y, float z) { this.x = x; this.y = y; this.z = z; this.w = 0F; }
        public Vector4(float x, float y) { this.x = x; this.y = y; this.z = 0F; this.w = 0F; }
        
        public void Set(float new_x, float new_y, float new_z, float new_w) { x = new_x; y = new_y; z = new_z; w = new_w; }
        
        public static Vector4 Lerp(Vector4 from, Vector4 to, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector4(
                from.x + (to.x - from.x) * t,
                from.y + (to.y - from.y) * t,
                from.z + (to.z - from.z) * t,
                from.w + (to.w - from.w) * t
            );
        }

        static public Vector4 MoveTowards(Vector4 current, Vector4 target, float maxDistanceDelta)
        {
            Vector4 toVector = target - current;
            float dist = toVector.magnitude;
            if (dist <= maxDistanceDelta || dist == 0)
                return target;
            return current + toVector / dist * maxDistanceDelta;
        }

        public static Vector4 Scale(Vector4 a, Vector4 b) { return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w); }
        public void Scale(Vector4 scale) { x *= scale.x; y *= scale.y; z *= scale.z; w *= scale.w; }


        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2) ^ (w.GetHashCode() >> 1);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector4)) return false;

            Vector4 rhs = (Vector4)other;
            return x.Equals(rhs.x) && y.Equals(rhs.y) && z.Equals(rhs.z) && w.Equals(rhs.w);
        }

        public static Vector4 Normalize(Vector4 a)
        {
            float mag = Magnitude(a);
            if (mag > kEpsilon)
                return a / mag;
            else
                return zero;
        }
        
        public void Normalize()
        {
            float mag = Magnitude(this);
            if (mag > kEpsilon)
                this = this / mag;
            else
                this = zero;
        }

        public Vector4 normalized { get { return Vector4.Normalize(this); } }
        
        override public string ToString()
        {
            return String.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", x, y, z, w);
        }
        public string ToString(string format)
        {
            return String.Format("({0}, {1}, {2}, {3})", x.ToString(format), y.ToString(format), z.ToString(format), w.ToString(format));
        }
        
        public static float Dot(Vector4 a, Vector4 b) { return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w; }

        public static Vector4 Project(Vector4 a, Vector4 b) { return b * Dot(a, b) / Dot(b, b); }
        
        public static float Distance(Vector4 a, Vector4 b) { return Magnitude(a - b); }

        public static float Magnitude(Vector4 a) { return Mathf.Sqrt(Dot(a, a)); }

        public float magnitude { get { return Mathf.Sqrt(Dot(this, this)); } }

        public static float SqrMagnitude(Vector4 a) { return Vector4.Dot(a, a); }
        public float SqrMagnitude() { return Dot(this, this); }

        public float sqrMagnitude { get { return Dot(this, this); } }
        
        public static Vector4 Min(Vector4 lhs, Vector4 rhs) { return new Vector4(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z), Mathf.Min(lhs.w, rhs.w)); }
        
        public static Vector4 Max(Vector4 lhs, Vector4 rhs) { return new Vector4(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z), Mathf.Max(lhs.w, rhs.w)); }

        public static Vector4 zero { get { return new Vector4(0F, 0F, 0F, 0F); } }
        public static Vector4 one { get { return new Vector4(1F, 1F, 1F, 1F); } }

        public static Vector4 Zero { get { return zero; } }
        
        public static Vector4 operator +(Vector4 a, Vector4 b) { return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w); }
        public static Vector4 operator -(Vector4 a, Vector4 b) { return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w); }
        public static Vector4 operator -(Vector4 a) { return new Vector4(-a.x, -a.y, -a.z, -a.w); }
        public static Vector4 operator *(Vector4 a, float d) { return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d); }
        public static Vector4 operator *(float d, Vector4 a) { return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d); }
        public static Vector4 operator /(Vector4 a, float d) { return new Vector4(a.x / d, a.y / d, a.z / d, a.w / d); }

        public static bool operator ==(Vector4 lhs, Vector4 rhs)
        {
            return SqrMagnitude(lhs - rhs) < kEpsilon * kEpsilon;
        }
        public static bool operator !=(Vector4 lhs, Vector4 rhs)
        {
            return SqrMagnitude(lhs - rhs) >= kEpsilon * kEpsilon;
        }

        /*
        public static implicit operator Vector4(Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 0.0F);
        }

        public static implicit operator Vector3(Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector4(Vector2 v)
        {
            return new Vector4(v.x, v.y, 0.0F, 0.0F);
        }

        public static implicit operator Vector2(Vector4 v)
        {
            return new Vector2(v.x, v.y);
        }
        */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Ray
    {
        private Vector3 m_Origin;
        private Vector3 m_Direction;


        public Ray(Vector3 origin, Vector3 direction) { m_Origin = origin; m_Direction = direction.normalized; }

        public Vector3 origin { get { return m_Origin; } set { m_Origin = value; } }
        public Vector3 direction { get { return m_Direction; } set { m_Direction = value.normalized; } }

        public Vector3 GetPoint(float distance) { return m_Origin + m_Direction * distance; }


        override public string ToString() { return String.Format("Origin: {0}, Dir: {1}", m_Origin, m_Direction); }
        public string ToString(string format)
        {
            return String.Format("Origin: {0}, Dir: {1}", m_Origin.ToString(format), m_Direction.ToString(format));
        }


    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Ray2D
    {
        private Vector2 m_Origin;
        private Vector2 m_Direction;


        public Ray2D(Vector2 origin, Vector2 direction) { m_Origin = origin; m_Direction = direction.normalized; }

        public Vector2 origin { get { return m_Origin; } set { m_Origin = value; } }


        public Vector2 direction { get { return m_Direction; } set { m_Direction = value.normalized; } }

        public Vector2 GetPoint(float distance) { return m_Origin + m_Direction * distance; }


        override public string ToString() { return String.Format("Origin: {0}, Dir: {1}", m_Origin, m_Direction); }


        public string ToString(string format)
        {
            return String.Format("Origin: {0}, Dir: {1}", m_Origin.ToString(format), m_Direction.ToString(format));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Plane
    {
        Vector3 m_Normal;
        float m_Distance;
        
        public Vector3 normal { get { return m_Normal; } set { m_Normal = value; } }
        public float distance { get { return m_Distance; } set { m_Distance = value; } }

        public Plane(Vector3 inNormal, Vector3 inPoint)
        {
            m_Normal = Vector3.Normalize(inNormal);
            m_Distance = -Vector3.Dot(inNormal, inPoint);
        }

        public Plane(Vector3 inNormal, float d)
        {
            m_Normal = Vector3.Normalize(inNormal);
            m_Distance = d;
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            m_Normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            m_Distance = -Vector3.Dot(m_Normal, a);
        }

        public void SetNormalAndPosition(Vector3 inNormal, Vector3 inPoint)
        {
            normal = Vector3.Normalize(inNormal);
            distance = -Vector3.Dot(inNormal, inPoint);
        }

        public void Set3Points(Vector3 a, Vector3 b, Vector3 c)
        {
            normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            distance = -Vector3.Dot(normal, a);
        }

        public float GetDistanceToPoint(Vector3 inPt) { return Vector3.Dot(normal, inPt) + distance; }

        public bool GetSide(Vector3 inPt) { return Vector3.Dot(normal, inPt) + distance > 0.0F; }

        public bool SameSide(Vector3 inPt0, Vector3 inPt1)
        {
            float d0 = GetDistanceToPoint(inPt0);
            float d1 = GetDistanceToPoint(inPt1);
            if (d0 > 0.0f && d1 > 0.0f)
                return true;
            else if (d0 <= 0.0f && d1 <= 0.0f)
                return true;
            else
                return false;
        }

        public bool Raycast(Ray ray, out float enter)
        {
            float vdot = Vector3.Dot(ray.direction, normal);
            float ndot = -Vector3.Dot(ray.origin, normal) - distance;

            if (Mathf.Approximately(vdot, 0.0f))
            {
                enter = 0.0F;
                return false;
            }

            enter = ndot / vdot;

            return enter > 0.0F;
        }
    }
}
