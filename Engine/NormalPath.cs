using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Normal path
    /// </summary>
    public class NormalPath : IControllerPath
    {
        /// <summary>
        /// Path check-points
        /// </summary>
        private readonly Vector3[] checkPoints = null;
        /// <summary>
        /// Path surface normals
        /// </summary>
        private readonly Vector3[] normals = null;

        /// <summary>
        /// First point
        /// </summary>
        public Vector3 First
        {
            get
            {
                return checkPoints[0];
            }
        }
        /// <summary>
        /// Last point
        /// </summary>
        public Vector3 Last
        {
            get
            {
                return checkPoints[^1];
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
                return checkPoints?.Length ?? 0;
            }
        }
        /// <summary>
        /// Number of normals in the path
        /// </summary>
        public int NormalCount
        {
            get
            {
                return normals?.Length ?? 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="checkPoints">Check-points</param>
        /// <param name="normals">Normals</param>
        public NormalPath(IEnumerable<Vector3> checkPoints, IEnumerable<Vector3> normals)
        {
            var points = checkPoints.ToArray();

            float length = 0;
            for (int i = 1; i < points.Length; i++)
            {
                length += Vector3.Distance(points[i], points[i - 1]);
            }

            this.checkPoints = points;
            this.normals = normals.ToArray();
            Length = length;
        }

        /// <summary>
        /// Gets the path position at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns the position at time</returns>
        public Vector3 GetPosition(float time)
        {
            if (PositionCount > 0)
            {
                if (time == 0) return checkPoints[0];
                if (time >= Length) return checkPoints[^1];

                Vector3 res = Vector3.Zero;
                float l = time;
                for (int i = 1; i < checkPoints.Length; i++)
                {
                    Vector3 segment = checkPoints[i] - checkPoints[i - 1];
                    float segmentLength = segment.Length();

                    if (l - segmentLength <= 0)
                    {
                        res = checkPoints[i - 1] + (Vector3.Normalize(segment) * l);

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
            if (NormalCount > 0)
            {
                if (time == 0) return normals[0];
                if (time >= Length) return normals[^1];

                Vector3 res = Vector3.Zero;
                float l = time;
                for (int i = 1; i < checkPoints.Length; i++)
                {
                    Vector3 segment = checkPoints[i] - checkPoints[i - 1];
                    float segmentLength = segment.Length();

                    if (l - segmentLength <= 0)
                    {
                        res = Vector3.Lerp(normals[i], normals[i - 1], l / segmentLength);

                        break;
                    }

                    l -= segmentLength;
                }

                return res;
            }
            else
            {
                return Vector3.Up;
            }
        }
        /// <summary>
        /// Gets the next control point at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns the next control path at specified time</returns>
        public Vector3 GetNextControlPoint(float time)
        {
            if (PositionCount > 0)
            {
                if (time == 0) return checkPoints[0];
                if (time >= Length) return checkPoints[^1];

                Vector3 res = Vector3.Zero;
                float l = time;
                for (int i = 1; i < checkPoints.Length; i++)
                {
                    Vector3 segment = checkPoints[i] - checkPoints[i - 1];
                    float segmentLength = segment.Length();

                    if (l - segmentLength <= 0)
                    {
                        res = checkPoints[i];

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
        /// Samples current path in a vector array
        /// </summary>
        /// <param name="sampleTime">Time delta</param>
        /// <returns>Returns a vector array</returns>
        public IEnumerable<Vector3> SamplePath(float sampleTime)
        {
            var returnPath = new List<Vector3>();

            float time = 0;
            while (time < Length)
            {
                returnPath.Add(GetPosition(time));

                time += sampleTime;
            }

            return returnPath.ToArray();
        }
    }
}
