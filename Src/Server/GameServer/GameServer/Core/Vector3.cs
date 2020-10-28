using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Common.Utils;
using System.ComponentModel;
using Microsoft.VisualBasic.Logging;

namespace GameServer.Core
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public const float kEpsilon = 1E-05f;
        public const float kEpsilonNormalSqrt = 1E-15f;
        public float x;
        public float y;
        public float z;
        private static readonly Vector3 zeroVector;
        private static readonly Vector3 oneVector;
        private static readonly Vector3 upVector;
        private static readonly Vector3 downVector;
        private static readonly Vector3 leftVector;
        private static readonly Vector3 rightVector;
        private static readonly Vector3 forwardVector;
        private static readonly Vector3 backVector;
        private static readonly Vector3 positiveInfinityVector;
        private static readonly Vector3 negativeInfinityVector;
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0f;
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            t = MathUtil.Clamp01(t);
            return new Vector3(a.x + ((b.x - a.x) * t), a.y + ((b.y - a.y) * t), a.z + ((b.z - a.z) * t));
        }

        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t) =>
            new Vector3(a.x + ((b.x - a.x) * t), a.y + ((b.y - a.y) * t), a.z + ((b.z - a.z) * t));

        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            Vector3 vector = target - current;
            float magnitude = vector.magnitude;
            if ((magnitude <= maxDistanceDelta) || (magnitude < float.Epsilon))
            {
                return target;
            }
            return (current + ((vector / magnitude) * maxDistanceDelta));
        }

    
        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed)
        {
            float deltaTime = Time.deltaTime;
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

      
        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime)
        {
            float deltaTime = Time.deltaTime;
            float positiveInfinity = float.PositiveInfinity;
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, positiveInfinity, deltaTime);
        }

        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
        {
            smoothTime = Math.Max(0.0001f, smoothTime);
            float num = 2f / smoothTime;
            float num2 = num * deltaTime;
            float num3 = 1f / (((1f + num2) + ((0.48f * num2) * num2)) + (((0.235f * num2) * num2) * num2));
            Vector3 vector = current - target;
            Vector3 vector2 = target;
            float maxLength = maxSpeed * smoothTime;
            vector = ClampMagnitude(vector, maxLength);
            target = current - vector;
            Vector3 vector3 = (currentVelocity + (num * vector)) * deltaTime;
            currentVelocity = (currentVelocity - (num * vector3)) * num3;
            Vector3 vector4 = target + ((vector + vector3) * num3);
            if (Dot(vector2 - current, vector4 - vector2) > 0f)
            {
                vector4 = vector2;
                currentVelocity = (vector4 - vector2) / deltaTime;
            }
            return vector4;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.x;

                    case 1:
                        return this.y;

                    case 2:
                        return this.z;
                }
                throw new IndexOutOfRangeException("Invalid Vector3 index!");
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;

                    case 1:
                        this.y = value;
                        break;

                    case 2:
                        this.z = value;
                        break;

                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }
        public void Set(float newX, float newY, float newZ)
        {
            this.x = newX;
            this.y = newY;
            this.z = newZ;
        }

        public static Vector3 Scale(Vector3 a, Vector3 b) =>
            new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);

        public void Scale(Vector3 scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
            this.z *= scale.z;
        }

        public static Vector3 Cross(Vector3 lhs, Vector3 rhs) =>
            new Vector3((lhs.y * rhs.z) - (lhs.z * rhs.y), (lhs.z * rhs.x) - (lhs.x * rhs.z), (lhs.x * rhs.y) - (lhs.y * rhs.x));

        public override int GetHashCode() =>
            ((this.x.GetHashCode() ^ (this.y.GetHashCode() << 2)) ^ (this.z.GetHashCode() >> 2));

        public override bool Equals(object other) =>
            ((other is Vector3) && this.Equals((Vector3)other));

        public bool Equals(Vector3 other) =>
            ((this.x.Equals(other.x) && this.y.Equals(other.y)) && this.z.Equals(other.z));

        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal) =>
            (((Vector3)((-2f * Dot(inNormal, inDirection)) * inNormal)) + inDirection);

        public static Vector3 Normalize(Vector3 value)
        {
            float num = Magnitude(value);
            if (num > 1E-05f)
            {
                return (value / num);
            }
            return zero;
        }

        public void Normalize()
        {
            float num = Magnitude(this);
            if (num > 1E-05f)
            {
                this /= num;
            }
            else
            {
                this = zero;
            }
        }

        public Vector3 normalized =>
            Normalize(this);
        public static float Dot(Vector3 lhs, Vector3 rhs) =>
            (((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z));

        //public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        //{
        //    float num = Dot(onNormal, onNormal);
        //    if (num < Math.Epsilon)
        //    {
        //        return zero;
        //    }
        //    return ((onNormal * Dot(vector, onNormal)) / num);
        //}

        //public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal) =>
        //    (vector - Project(vector, planeNormal));

        public static float Angle(Vector3 from, Vector3 to)
        {
            float num = (float)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (num < 1E-15f)
            {
                return 0f;
            }
            return (float)(Math.Acos(MathUtil.Clamp((float)(Dot(from, to) / num), (float)-1f, (float)1f)) * 57.29578f);
        }

        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float num = Angle(from, to);
            float num2 = Math.Sign(Dot(axis, Cross(from, to)));
            return (num * num2);
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return (float)Math.Sqrt(((vector.x * vector.x) + (vector.y * vector.y)) + (vector.z * vector.z));
        }

        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
            if (vector.sqrMagnitude > (maxLength * maxLength))
            {
                return (vector.normalized * maxLength);
            }
            return vector;
        }

        public static float Magnitude(Vector3 vector) =>
           (float) Math.Sqrt(((vector.x * vector.x) + (vector.y * vector.y)) + (vector.z * vector.z));

        public float magnitude =>
             (float)Math.Sqrt(((this.x * this.x) + (this.y * this.y)) + (this.z * this.z));
        public static float SqrMagnitude(Vector3 vector) =>
            (((vector.x * vector.x) + (vector.y * vector.y)) + (vector.z * vector.z));

        public float sqrMagnitude =>
            (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z));
        public static Vector3 Min(Vector3 lhs, Vector3 rhs) =>
            new Vector3(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));

        public static Vector3 Max(Vector3 lhs, Vector3 rhs) =>
            new Vector3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));

        public static Vector3 zero =>
            zeroVector;
        public static Vector3 one =>
            oneVector;
        public static Vector3 forward =>
            forwardVector;
        public static Vector3 back =>
            backVector;
        public static Vector3 up =>
            upVector;
        public static Vector3 down =>
            downVector;
        public static Vector3 left =>
            leftVector;
        public static Vector3 right =>
            rightVector;
        public static Vector3 positiveInfinity =>
            positiveInfinityVector;
        public static Vector3 negativeInfinity =>
            negativeInfinityVector;
        public static Vector3 operator +(Vector3 a, Vector3 b) =>
            new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3 operator -(Vector3 a, Vector3 b) =>
            new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vector3 operator -(Vector3 a) =>
            new Vector3(-a.x, -a.y, -a.z);

        public static Vector3 operator *(Vector3 a, float d) =>
            new Vector3(a.x * d, a.y * d, a.z * d);

        public static Vector3 operator *(float d, Vector3 a) =>
            new Vector3(a.x * d, a.y * d, a.z * d);

        public static Vector3 operator /(Vector3 a, float d) =>
            new Vector3(a.x / d, a.y / d, a.z / d);

        public static bool operator ==(Vector3 lhs, Vector3 rhs) =>
            (SqrMagnitude(lhs - rhs) < 9.999999E-11f);

        public static bool operator !=(Vector3 lhs, Vector3 rhs) =>
            !(lhs == rhs);

        public override string ToString()
        {
            object[] args = new object[] { this.x, this.y, this.z };
            return string.Format("({0:F1}, {1:F1}, {2:F1})", args);
        }

        public string ToString(string format)
        {
            object[] args = new object[] { this.x.ToString(format), this.y.ToString(format), this.z.ToString(format) };
            return string.Format("({0}, {1}, {2})", args);
        }

        [Obsolete("Use Vector3.forward instead.")]
        public static Vector3 fwd =>
            new Vector3(0f, 0f, 1f);
        [Obsolete("Use Vector3.Angle instead. AngleBetween uses radians instead of degrees and was deprecated for this reason")]
        public static float AngleBetween(Vector3 from, Vector3 to) =>
            (float)Math.Acos(MathUtil.Clamp(Dot(from.normalized, to.normalized), -1f, 1f));

        //[Obsolete("Use Vector3.ProjectOnPlane instead.")]
        //public static Vector3 Exclude(Vector3 excludeThis, Vector3 fromThat) =>
        //    ProjectOnPlane(fromThat, excludeThis);
        public static implicit operator Vector3(Vector3Int v)
        {
            return new Vector3() { x = v.x, y = v.y, z = v.z };
        }
        public static implicit operator Vector3Int(Vector3 v)
        {
            return new Vector3Int((int)v.x, (int)v.y, (int)v.z );
        }
        static Vector3()
        {
            zeroVector = new Vector3(0f, 0f, 0f);
            oneVector = new Vector3(1f, 1f, 1f);
            upVector = new Vector3(0f, 1f, 0f);
            downVector = new Vector3(0f, -1f, 0f);
            leftVector = new Vector3(-1f, 0f, 0f);
            rightVector = new Vector3(1f, 0f, 0f);
            forwardVector = new Vector3(0f, 0f, 1f);
            backVector = new Vector3(0f, 0f, -1f);
            positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        }

     
    }
}
