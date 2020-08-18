using SharpDX;

namespace Engine
{
    /// <summary>
    /// 2D Line
    /// </summary>
    public class Line2D
    {
        /// <summary>
        /// Start point
        /// </summary>
        public Vector2 Point1 { get; set; }
        /// <summary>
        /// End point
        /// </summary>
        public Vector2 Point2 { get; set; }
        /// <summary>
        /// Length
        /// </summary>
        public float Length
        {
            get
            {
                return Vector2.Distance(this.Point1, this.Point2);
            }
        }
        /// <summary>
        /// Direction vector of the line
        /// </summary>
        public Vector2 Direction
        {
            get
            {
                return Vector2.Normalize(this.Point2 - this.Point1);
            }
        }

        /// <summary>
        /// Transform line coordinates
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new line</returns>
        public static Line2D Transform(Line2D line, Matrix transform)
        {
            return new Line2D(
                Vector2.TransformCoordinate(line.Point1, transform),
                Vector2.TransformCoordinate(line.Point2, transform));
        }
        /// <summary>
        /// Transform line list coordinates
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new line list</returns>
        public static Line2D[] Transform(Line2D[] lines, Matrix transform)
        {
            Line2D[] trnLines = new Line2D[lines.Length];

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
        /// <param name="x2">X coordinate of end point</param>
        /// <param name="y2">Y coordinate of end point</param>
        public Line2D(float x1, float y1, float x2, float y2)
        {
            this.Point1 = new Vector2(x1, y1);
            this.Point2 = new Vector2(x2, y2);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        public Line2D(Vector2 p1, Vector2 p2)
        {
            this.Point1 = p1;
            this.Point2 = p2;
        }

        /// <summary>
        /// Text representation
        /// </summary>
        public override string ToString()
        {
            return $"P1({this.Point1}) -> P2({this.Point2});";
        }
    }
}
