using SharpDX;

namespace TerrainSamples.SceneGrid.Rules
{
    /// <summary>
    /// Area
    /// </summary>
    public class Area
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Direction
        /// </summary>
        public Vector3 Direction { get; set; }
        /// <summary>
        /// Angle
        /// </summary>
        public float Angle { get; set; }
        /// <summary>
        /// Distance
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// Gets bounding frustum
        /// </summary>
        /// <returns>Returns bounding frustum</returns>
        public BoundingFrustum GetFrustum()
        {
            return BoundingFrustum.FromCamera(
                Position,
                Direction,
                Vector3.Up,
                Angle,
                1,
                Distance,
                1);
        }
    }
}
