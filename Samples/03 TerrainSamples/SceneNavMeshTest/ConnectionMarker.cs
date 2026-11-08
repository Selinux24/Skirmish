namespace TerrainSamples.SceneNavMeshTest
{
    using Engine.Mathematics;

    struct ConnectionMarker
    {
        public int Id { get; set; }
        public Vector3 From { get; set; }
        public Vector3 To { get; set; }
        public float Radius { get; set; }
        public bool BiDirectional { get; set; }

        public readonly bool IntersectsRay(Ray ray)
        {
            var from = From;
            var up = Vector3.Up;
            if (Collision.RayIntersectsCircle3D(ref ray, ref from, ref up, Radius, out _, out _))
            {
                return true;
            }

            var to = To;
            if (Collision.RayIntersectsCircle3D(ref ray, ref to, ref up, Radius, out _, out _))
            {
                return true;
            }

            return false;
        }
    }
}
