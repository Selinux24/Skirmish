using SharpDX;

namespace Engine.Physics.GJKEPA
{
    public struct MinkowskiDifference
    {
        public ISupportMappable SupportA, SupportB;
        public Matrix OrientationA, OrientationB;
        public Vector3 PositionA, PositionB;

        private void SupportMapTransformedA(in Vector3 direction, out Vector3 result)
        {
            Matrix transposed = Matrix.Transpose(OrientationA);
            Vector3 tmp = Vector3.TransformCoordinate(direction, transposed);
            SupportA.SupportMapping(tmp, out result);
            result = Vector3.TransformCoordinate(result, OrientationA);
            result = Vector3.Add(result, PositionA);
        }

        private void SupportMapTransformedB(in Vector3 direction, out Vector3 result)
        {
            Matrix transposed = Matrix.Transpose(OrientationB);
            Vector3 tmp = Vector3.TransformCoordinate(direction, transposed);
            SupportB.SupportMapping(tmp, out result);
            result = Vector3.TransformCoordinate(result, OrientationB);
            result = Vector3.Add(result, PositionB);
        }

        public void Support(in Vector3 direction, out Vector3 vA, out Vector3 vB, out Vector3 v)
        {
            Vector3 tmp = Vector3.Negate(direction);
            SupportMapTransformedA(tmp, out vA);
            SupportMapTransformedB(direction, out vB);
            v = Vector3.Subtract(vA, vB);
        }

        public void SupportMapping(in Vector3 direction, out Vector3 result)
        {
            Support(direction, out _, out _, out result);
        }

        public void SupportCenter(out Vector3 center)
        {
            center = Vector3.Subtract(PositionA, PositionB);
        }
    }
}
