using Engine;
using SharpDX;

namespace TerrainSamples.SceneNavMeshTest
{
    struct ObstacleMarker
    {
        public int Id { get; set; }
        public BoundingCylinder Obstacle { get; set; }
        public readonly BoundingBox Bbox
        {
            get
            {
                var center = Obstacle.Center;
                var extents = new Vector3(Obstacle.Radius, Obstacle.Height * 0.5f, Obstacle.Radius);
                return new(-extents + center, extents + center);
            }
        }
    }
}
