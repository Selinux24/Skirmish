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

        /// <inheritdoc/>
        public Vector3 First
        {
            get
            {
                return path[0];
            }
        }
        /// <inheritdoc/>
        public Vector3 Last
        {
            get
            {
                return path[^1];
            }
        }
        /// <inheritdoc/>
        public float Length { get; private set; }
        /// <inheritdoc/>
        public int PositionCount
        {
            get
            {
                return path?.Length ?? 0;
            }
        }
        /// <inheritdoc/>
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
            InitializePath(origin, null, destination);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="path">Inner path</param>
        /// <param name="destination">Destination</param>
        public SegmentPath(Vector3 origin, IEnumerable<Vector3> path, Vector3 destination)
        {
            InitializePath(origin, path, destination);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="path">Path</param>
        public SegmentPath(Vector3 origin, IEnumerable<Vector3> path)
        {
            InitializePath(origin, path, null);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="destination">Destination</param>
        public SegmentPath(Vector3[] path, Vector3 destination)
        {
            InitializePath(null, path, destination);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path</param>
        public SegmentPath(Vector3[] path)
        {
            InitializePath(null, path, null);
        }

        /// <summary>
        /// Initializes a path
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="path">Inner path</param>
        /// <param name="destination">Destination</param>
        private void InitializePath(Vector3? origin, IEnumerable<Vector3> path, Vector3? destination)
        {
            var lPath = new List<Vector3>();

            if (origin.HasValue) lPath.Add(origin.Value);
            if (path != null) lPath.AddRange(path);
            if (destination.HasValue) lPath.Add(destination.Value);

            float length = 0;
            for (int i = 1; i < lPath.Count; i++)
            {
                length += Vector3.Distance(lPath[i], lPath[i - 1]);
            }

            this.path = [.. lPath];
            Length = length;
        }
        /// <inheritdoc/>
        public Vector3 GetPosition(float time)
        {
            if (PositionCount <= 0)
            {
                return Vector3.Zero;
            }

            if (MathUtil.IsZero(time))
            {
                return path[0];
            }

            if (time >= Length)
            {
                return path[^1];
            }

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
        /// <inheritdoc/>
        public Vector3 GetNormal(float time)
        {
            return Vector3.Up;
        }
        /// <inheritdoc/>
        public Vector3 GetNextControlPoint(float time)
        {
            if (PositionCount <= 0)
            {
                return Vector3.Zero;
            }

            if (MathUtil.IsZero(time))
            {
                return path[0];
            }

            if (time >= Length)
            {
                return path[^1];
            }

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
        /// <inheritdoc/>
        public IEnumerable<Vector3> SamplePath(float sampleTime)
        {
            List<Vector3> returnPath = [];

            float time = 0;
            while (time < Length)
            {
                returnPath.Add(GetPosition(time));

                time += sampleTime;
            }

            return returnPath;
        }
    }
}
