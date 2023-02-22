using SharpDX;

namespace Engine
{
    /// <summary>
    /// Segment
    /// </summary>
    public struct Segment
    {
        /// <summary>
        /// First point
        /// </summary>
        public Vector3 Point1 { get; set; }
        /// <summary>
        /// Second point
        /// </summary>
        public Vector3 Point2 { get; set; }
        /// <summary>
        /// Segment length
        /// </summary>
        public float Length
        {
            get
            {
                return Vector3.Distance(Point1, Point2);
            }
        }
        /// <summary>
        /// Segment squared length
        /// </summary>
        public float LengthSquared
        {
            get
            {
                return Vector3.DistanceSquared(Point1, Point2);
            }
        }
        /// <summary>
        /// Gets the normalized direction vector
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return Vector3.Normalize(Point2 - Point1);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Segment()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public Segment(Vector3 point1, Vector3 point2)
        {
            Point1 = point1;
            Point2 = point2;
        }
    }
}
