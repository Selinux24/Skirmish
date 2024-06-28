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

        /// <inheritdoc/>
        public Vector3 First
        {
            get
            {
                return checkPoints[0];
            }
        }
        /// <inheritdoc/>
        public Vector3 Last
        {
            get
            {
                return checkPoints[^1];
            }
        }
        /// <inheritdoc/>
        public float Length { get; private set; }
        /// <inheritdoc/>
        public int PositionCount
        {
            get
            {
                return checkPoints?.Length ?? 0;
            }
        }
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Vector3 GetPosition(float time)
        {
            if (PositionCount <= 0)
            {
                return Vector3.Zero;
            }

            if (MathUtil.IsZero(time))
            {
                return checkPoints[0];
            }

            if (time >= Length)
            {
                return checkPoints[^1];
            }

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
        /// <inheritdoc/>
        public Vector3 GetNormal(float time)
        {
            if (NormalCount <= 0)
            {
                return Vector3.Up;
            }

            if (MathUtil.IsZero(time))
            {
                return normals[0];
            }

            if (time >= Length)
            {
                return normals[^1];
            }

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
        /// <inheritdoc/>
        public Vector3 GetNextControlPoint(float time)
        {
            if (PositionCount <= 0)
            {
                return Vector3.Zero;
            }

            if (MathUtil.IsZero(time))
            {
                return checkPoints[0];
            }

            if (time >= Length)
            {
                return checkPoints[^1];
            }

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
