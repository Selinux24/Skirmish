using SharpDX;

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
                return this.checkPoints != null ? this.checkPoints.Length : 0;
            }
        }
        /// <summary>
        /// Number of normals in the path
        /// </summary>
        public int NormalCount
        {
            get
            {
                return this.normals != null ? this.normals.Length : 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="checkPoints">Check-points</param>
        /// <param name="normals">Normals</param>
        public NormalPath(Vector3[] checkPoints, Vector3[] normals)
        {
            float length = 0;
            for (int i = 1; i < checkPoints.Length; i++)
            {
                length += Vector3.Distance(checkPoints[i], checkPoints[i - 1]);
            }

            this.checkPoints = checkPoints;
            this.normals = normals;
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
                if (time == 0) return checkPoints[0];
                if (time >= this.Length) return checkPoints[checkPoints.Length - 1];

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
            if (this.NormalCount > 0)
            {
                if (time == 0) return normals[0];
                if (time >= this.Length) return normals[normals.Length - 1];

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
            if (this.PositionCount > 0)
            {
                if (time == 0) return checkPoints[0];
                if (time >= this.Length) return checkPoints[checkPoints.Length - 1];

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
    }
}
