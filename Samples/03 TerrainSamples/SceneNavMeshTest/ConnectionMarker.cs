using Engine.Common;
using SharpDX;

namespace TerrainSamples.SceneNavMeshTest
{
    struct ConnectionMarker
    {
        public int Id { get; set; }
        public Vector3 From { get; set; }
        public Vector3 To { get; set; }
        public float Radius { get; set; }
        public bool BiDirectional { get; set; }

        public readonly bool IntersectsRay(Ray ray)
        {
            if (Intersection.RayIntersectsCircle3D(ref ray, From, Vector3.Up, Radius))
            {
                return true;
            }

            if (Intersection.RayIntersectsCircle3D(ref ray, To, Vector3.Up, Radius))
            {
                return true;
            }

            return false;
        }
    }
}
