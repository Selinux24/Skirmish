using SharpDX;

namespace Engine
{
    /// <summary>
    /// 3D Line
    /// </summary>
    public struct Line3
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
        /// Transform line coordinates
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new line</returns>
        public static Line3 Transform(Line3 line, Matrix transform)
        {
            return new Line3(
                Vector3.TransformCoordinate(line.Point1, transform),
                Vector3.TransformCoordinate(line.Point2, transform));
        }
        /// <summary>
        /// Transform line list coordinates
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new line list</returns>
        public static Line3[] Transform(Line3[] lines, Matrix transform)
        {
            Line3[] trnLines = new Line3[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                trnLines[i] = Transform(lines[i], transform);
            }

            return trnLines;
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
        public Line3(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            this.Point1 = new Vector3(x1, y1, z1);
            this.Point2 = new Vector3(x2, y2, z2);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        public Line3(Vector3 p1, Vector3 p2)
        {
            this.Point1 = p1;
            this.Point2 = p2;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ray">Ray</param>
        public Line3(Ray ray)
        {
            this.Point1 = ray.Position;
            this.Point2 = ray.Position + ray.Direction;
        }

        /// <summary>
        /// Text representation
        /// </summary>
        public override string ToString()
        {
            return string.Format("Vertex 1 {0}; Vertex 2 {1};", this.Point1, this.Point2);
        }
    }
}
