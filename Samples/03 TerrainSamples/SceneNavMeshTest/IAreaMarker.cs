using Engine.Common;
using SharpDX;
using System.Collections.Generic;

namespace TerrainSamples.SceneNavMeshTest
{
    interface IAreaMarker
    {
        int Id { get; set; }

        bool IntersectsRay(Ray ray);
    }

    struct CylinderAreaMarker : IAreaMarker
    {
        public int Id { get; set; }
        public Vector3 Center { get; set; }
        public float Radius { get; set; }
        public float MinH { get; set; }
        public float MaxH { get; set; }

        public readonly bool IntersectsRay(Ray ray)
        {
            if (Intersection.RayIntersectsCircle3D(ref ray, Center, Vector3.Up, Radius))
            {
                return true;
            }

            return false;
        }
    }

    struct ConvexAreaMarker : IAreaMarker
    {
        public int Id { get; set; }
        public Vector3[] Vertices { get; set; }
        public float MinH { get; set; }
        public float MaxH { get; set; }

        public readonly BoundingBox GetBounds()
        {
            return BoundingBox.FromPoints(Vertices);
        }

        public readonly bool IntersectsRay(Ray ray)
        {
            var cvBox = GetBounds();

            return cvBox.Intersects(ref ray);
        }

        public readonly IEnumerable<(Vector3[] verts, bool ccw)> GetPolygons()
        {
            Vector3[] v = Vertices;
            float minY = MinH;
            float maxY = MaxH;

            //Top
            Vector3[] topFace = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                topFace[i] = new(v[i].X, v[i].Y + maxY, v[i].Z);
            }
            yield return (topFace, true);

            //N Sides
            Vector3[] sideFace = new Vector3[4];
            for (int s = 0; s < v.Length; s++)
            {
                int ns = (s + 1) % v.Length;

                int i = 0;
                sideFace[i++] = new(v[s].X, v[s].Y + minY, v[s].Z);
                sideFace[i++] = new(v[ns].X, v[ns].Y + minY, v[ns].Z);
                sideFace[i++] = new(v[ns].X, v[ns].Y + maxY, v[ns].Z);
                sideFace[i] = new(v[s].X, v[s].Y + maxY, v[s].Z);

                yield return ([.. sideFace], true);
            }

            //Bottom
            Vector3[] bottomFace = new Vector3[v.Length];
            for (int i = v.Length - 1; i >= 0; i--)
            {
                bottomFace[i] = new(v[i].X, v[i].Y + minY, v[i].Z);
            }
            yield return (bottomFace, false);
        }
    }
}
