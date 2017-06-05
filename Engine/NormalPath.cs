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
        private Vector3[] checkPoints = null;
        /// <summary>
        /// Path surface normals
        /// </summary>
        private Vector3[] normals = null;
        /// <summary>
        /// Gets the total length of the path
        /// </summary>
        public float Length { get; private set; }

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
        /// Gets the path position at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns the position at distance</returns>
        public Vector3 GetPosition(float distance)
        {
            if (distance == 0) return checkPoints[0];
            if (distance >= this.Length) return checkPoints[checkPoints.Length - 1];

            Vector3 res = Vector3.Zero;
            float l = distance;
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
        /// <summary>
        /// Gets path normal in specified time
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns path normal</returns>
        public Vector3 GetNormal(float distance)
        {
            if (distance == 0) return normals[0];
            if (distance >= this.Length) return normals[normals.Length - 1];

            Vector3 res = Vector3.Zero;
            float l = distance;
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
        /// <summary>
        /// Gets the next control point at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns the next control path at specified distance</returns>
        public Vector3 GetNextControlPoint(float distance)
        {
            if (distance == 0) return checkPoints[0];
            if (distance >= this.Length) return checkPoints[checkPoints.Length - 1];

            Vector3 res = Vector3.Zero;
            float l = distance;
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
    }
}
