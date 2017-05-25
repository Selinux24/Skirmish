using SharpDX;

namespace Engine
{
    /// <summary>
    /// Segment path controller
    /// </summary>
    public class SegmentPath : IControllerPath
    {
        /// <summary>
        /// Path
        /// </summary>
        private Vector3[] path = null;
        /// <summary>
        /// Gets the total length of the path
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path</param>
        public SegmentPath(Vector3[] path)
        {
            this.path = path;

            float length = 0;
            for (int i = 1; i < path.Length; i++)
            {
                length += Vector3.Distance(path[i], path[i - 1]);
            }

            this.Length = length;
        }

        /// <summary>
        /// Gets the path position at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns the position at distance</returns>
        public Vector3 GetPosition(float distance)
        {
            if (distance == 0) return path[0];
            if (distance == this.Length) return path[path.Length - 1];

            float l = distance;
            for (int i = 1; i < path.Length; i++)
            {
                Vector3 segment = path[i] - path[i - 1];
                float segmentLength = segment.Length();

                if (l - segmentLength <= 0)
                {
                    return path[i - 1] + (Vector3.Normalize(segment) * l);
                }

                l -= segmentLength;
            }

            return Vector3.Zero;
        }
        /// <summary>
        /// Gets the next control point at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns the next control path at specified distance</returns>
        public Vector3 GetNextControlPoint(float distance)
        {
            if (distance == 0) return path[0];
            if (distance == this.Length) return path[path.Length - 1];

            float l = distance;
            for (int i = 1; i < path.Length; i++)
            {
                Vector3 segment = path[i] - path[i - 1];
                float segmentLength = segment.Length();

                if (l - segmentLength <= 0)
                {
                    return path[i];
                }

                l -= segmentLength;
            }

            return Vector3.Zero;
        }
    }
}
