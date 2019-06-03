using SharpDX;
using System.Collections.Generic;

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
        /// First point
        /// </summary>
        public Vector3 First
        {
            get
            {
                return this.path[0];
            }
        }
        /// <summary>
        /// Last point
        /// </summary>
        public Vector3 Last
        {
            get
            {
                return this.path[this.path.Length - 1];
            }
        }
        /// <summary>
        /// Gets the total length of the path
        /// </summary>
        public float Length { get; private set; }
        /// <summary>
        /// Gets the total checkpoint number of the path
        /// </summary>
        public int PositionCount
        {
            get
            {
                return this.path != null ? this.path.Length : 0;
            }
        }
        /// <summary>
        /// Number of normals in the path
        /// </summary>
        public int NormalCount
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="destination">Destination</param>
        public SegmentPath(Vector3 origin, Vector3 destination)
        {
            this.InitializePath(origin, null, destination);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="path">Inner path</param>
        /// <param name="destination">Destination</param>
        public SegmentPath(Vector3 origin, Vector3[] path, Vector3 destination)
        {
            this.InitializePath(origin, path, destination);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="path">Path</param>
        public SegmentPath(Vector3 origin, Vector3[] path)
        {
            this.InitializePath(origin, path, null);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="destination">Destination</param>
        public SegmentPath(Vector3[] path, Vector3 destination)
        {
            this.InitializePath(null, path, destination);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path</param>
        public SegmentPath(Vector3[] path)
        {
            this.InitializePath(null, path, null);
        }

        /// <summary>
        /// Initializes a path
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="path">Inner path</param>
        /// <param name="destination">Destination</param>
        private void InitializePath(Vector3? origin, Vector3[] path, Vector3? destination)
        {
            List<Vector3> lPath = new List<Vector3>();

            if (origin.HasValue) lPath.Add(origin.Value);
            if (path != null) lPath.AddRange(path);
            if (destination.HasValue) lPath.Add(destination.Value);

            float length = 0;
            for (int i = 1; i < lPath.Count; i++)
            {
                length += Vector3.Distance(lPath[i], lPath[i - 1]);
            }

            this.path = lPath.ToArray();
            this.Length = length;
        }
        /// <summary>
        /// Gets the path position at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns the position at time</returns>
        public Vector3 GetPosition(float time)
        {
            if (this.PositionCount > 0)
            {
                if (time == 0) return path[0];
                if (time >= this.Length) return path[path.Length - 1];

                Vector3 res = Vector3.Zero;
                float l = time;
                for (int i = 1; i < path.Length; i++)
                {
                    Vector3 segment = path[i] - path[i - 1];
                    float segmentLength = segment.Length();

                    if (l - segmentLength <= 0)
                    {
                        res = path[i - 1] + (Vector3.Normalize(segment) * l);

                        break;
                    }

                    l -= segmentLength;
                }

                return res;
            }
            else
            {
                return Vector3.Zero;
            }
        }
        /// <summary>
        /// Gets path normal in specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns path normal</returns>
        public Vector3 GetNormal(float time)
        {
            return Vector3.Up;
        }
        /// <summary>
        /// Gets the next control point at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns the next control path at specified time</returns>
        public Vector3 GetNextControlPoint(float time)
        {
            if (this.PositionCount > 0)
            {
                if (time == 0) return path[0];
                if (time >= this.Length) return path[path.Length - 1];

                Vector3 res = Vector3.Zero;
                float l = time;
                for (int i = 1; i < path.Length; i++)
                {
                    Vector3 segment = path[i] - path[i - 1];
                    float segmentLength = segment.Length();

                    if (l - segmentLength <= 0)
                    {
                        res = path[i];

                        break;
                    }

                    l -= segmentLength;
                }

                return res;
            }
            else
            {
                return Vector3.Zero;
            }
        }
    }
}
