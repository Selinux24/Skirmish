using SharpDX;

namespace GameLogic.Rules
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
                this.Position,
                this.Direction,
                Vector3.Up,
                this.Angle,
                1,
                this.Distance,
                1);
        }
    }
}
