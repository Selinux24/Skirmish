using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Line
    /// </summary>
    public struct Line
    {
        /// <summary>
        /// Start point
        /// </summary>
        public Vector3 Point1;
        /// <summary>
        /// End point
        /// </summary>
        public Vector3 Point2;
        /// <summary>
        /// Length
        /// </summary>
        public float Length
        {
            get
            {
                return Vector3.Distance(this.Point1, this.Point2);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x1">X coordinate of start point</param>
        /// <param name="y1">Y coordinate of start point</param>
        /// <param name="z1">Z coordinate of start point</param>
        /// <param name="x2">X coordinate of end point</param>
        /// <param name="y2">Y coordinate of end point</param>
        /// <param name="z2">Z coordinate of end point</param>
        public Line(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            this.Point1 = new Vector3(x1, y1, z1);
            this.Point2 = new Vector3(x2, y2, z2);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        public Line(Vector3 p1, Vector3 p2)
        {
            this.Point1 = p1;
            this.Point2 = p2;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ray">Ray</param>
        public Line(Ray ray)
        {
            this.Point1 = ray.Position;
            this.Point2 = ray.Position + ray.Direction;
        }
    }
}
