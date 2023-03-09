using SharpDX;

namespace Engine.Physics.GJKEPA
{
    [Shape("Cylinder")]
    public class CylinderShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            ShapeHelper.SupportDisc(direction, out Vector3 res1);
            ShapeHelper.SupportLine(direction, out Vector3 res2);
            result = res1 + res2;
        }
    }

    [Shape("Cube")]
    public class CubeShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            ShapeHelper.SupportCube(direction, out result);
            result *= 0.5f;
        }
    }

    [Shape("Line Segment")]
    public class LineShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            ShapeHelper.SupportLine(direction, out Vector3 res1);
            ShapeHelper.SupportSphere(direction, out Vector3 res2);
            result = res1 + res2 * 0.01f;
        }
    }

    [Shape("Triangle")]
    public class TriangleShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            ShapeHelper.SupportTriangle(direction, out Vector3 res1);
            ShapeHelper.SupportSphere(direction, out Vector3 res2);
            result = res1 + res2 * 0.01f;
        }
    }

    [Shape("Sphere")]
    public class SphereShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            ShapeHelper.SupportSphere(direction, out result);
            result *= 0.5f;
        }
    }

    [Shape("Ellipsoid")]
    public class EllipsoidShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            // ellipsoid == affine transformation of a sphere
            Vector3 dir = direction;
            dir.X *= 0.5f; dir.Y *= 0.8f; dir.Z *= 0.2f;
            ShapeHelper.SupportSphere(dir, out result);
            result.X *= 0.5f; result.Y *= 0.8f; result.Z *= 0.2f;
        }
    }

    [Shape("Capped Cone")]
    public class CappedConeShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            ShapeHelper.SupportDisc(direction, out Vector3 res1);
            ShapeHelper.SupportCone(direction, out Vector3 res2);
            result = (res1 + res2) * 0.5f;
        }
    }

    [Shape("Capsule")]
    public class CapsuleShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            ShapeHelper.SupportLine(direction, out Vector3 res1);
            ShapeHelper.SupportSphere(direction, out Vector3 res2);
            result = (res1 + res2 * 0.5f) * 0.8f;
        }
    }

    [Shape("Cone")]
    public class ConeShape : ISupportMappable
    {
        public void SupportMapping(Vector3 direction, out Vector3 result)
        {
            ShapeHelper.SupportCone(direction, out result);
        }
    }
}
