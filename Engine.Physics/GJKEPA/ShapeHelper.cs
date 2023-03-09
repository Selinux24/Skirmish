using SharpDX;
using System;

namespace Engine.Physics.GJKEPA
{
    public static class ShapeHelper
    {
        public static void SupportLine(in Vector3 direction, out Vector3 result)
        {
            Vector3 a = new Vector3(0, 0.5f, 0);
            Vector3 b = new Vector3(0, -0.5f, 0);

            float t0 = Vector3.Dot(direction, a);
            float t2 = Vector3.Dot(direction, b);

            if (t0 > t2) result = a;
            else result = b;
        }

        public static void SupportTriangle(in Vector3 direction, out Vector3 result)
        {
            Vector3 a = new Vector3(0, 0, 1);
            Vector3 b = new Vector3(-1, 0, -1);
            Vector3 c = new Vector3(1, 0, -1);

            float t0 = Vector3.Dot(direction, a);
            float t1 = Vector3.Dot(direction, b);
            float t2 = Vector3.Dot(direction, c);

            if (t0 > t1) result = t0 > t2 ? a : c;
            else result = t2 > t1 ? c : b;
        }

        public static void SupportDisc(in Vector3 direction, out Vector3 result)
        {
            result.X = direction.X;
            result.Y = 0;
            result.Z = direction.Z;

            if (result.LengthSquared() > 1e-12) result.Normalize();
            result *= 0.5f;
        }

        public static void SupportSphere(in Vector3 direction, out Vector3 result)
        {
            result = direction;
            result.Normalize();
        }

        public static void SupportCone(in Vector3 direction, out Vector3 result)
        {
            SupportDisc(direction, out Vector3 res1);
            Vector3 res2 = new Vector3(0, 1, 0);

            if (Vector3.Dot(direction, res1) >= Vector3.Dot(direction, res2)) result = res1;
            else result = res2;

            result.Y -= 0.5f;
        }

        public static void SupportCube(in Vector3 direction, out Vector3 result)
        {
            result.X = Math.Sign(direction.X);
            result.Y = Math.Sign(direction.Y);
            result.Z = Math.Sign(direction.Z);
        }
    }
}
