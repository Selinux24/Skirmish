using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Curve 3D controller path
    /// </summary>
    public class Curve3D : IControllerPath
    {
        private readonly Curve cX = new();
        private readonly Curve cY = new();
        private readonly Curve cZ = new();

        /// <summary>
        /// Returns <c>true</c> if this curve is constant (has zero or one points); <c>false</c> otherwise.
        /// </summary>
        public bool IsConstant
        {
            get { return PositionCount <= 1; }
        }
        /// <summary>
        /// Defines how to handle weighting values that are greater than the last control point in the curve.
        /// </summary>
        public CurveLoopType PostLoop
        {
            get
            {
                return cX.PostLoop;
            }
            set
            {
                cX.PostLoop = value;
                cY.PostLoop = value;
                cZ.PostLoop = value;
            }
        }
        /// <summary>
        /// Defines how to handle weighting values that are less than the first control point in the curve.
        /// </summary>
        public CurveLoopType PreLoop
        {
            get
            {
                return cX.PreLoop;
            }
            set
            {
                cX.PreLoop = value;
                cY.PreLoop = value;
                cZ.PreLoop = value;
            }
        }
        /// <summary>
        /// Gets the key count
        /// </summary>
        public int PositionCount
        {
            get
            {
                return cX.Keys.Count;
            }
        }
        /// <summary>
        /// Number of normals in the curve
        /// </summary>
        public int NormalCount
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// First point
        /// </summary>
        public Vector3 First
        {
            get
            {
                return Start.Value;
            }
        }
        /// <summary>
        /// Last point
        /// </summary>
        public Vector3 Last
        {
            get
            {
                return End.Value;
            }
        }

        /// <summary>
        /// Gets the starting key
        /// </summary>
        public Curve3DKey Start
        {
            get
            {
                return GetKey(0);
            }
        }
        /// <summary>
        /// Gets the key collection
        /// </summary>
        public Curve3DKey[] Keys
        {
            get
            {
                Curve3DKey[] keys = new Curve3DKey[PositionCount];

                for (int i = 0; i < PositionCount; i++)
                {
                    keys[i] = GetKey(i);
                }

                return keys;
            }
        }
        /// <summary>
        /// Gets the ending key
        /// </summary>
        public Curve3DKey End
        {
            get
            {
                return GetKey(PositionCount - 1);
            }
        }
        /// <summary>
        /// Gets the curve total length
        /// </summary>
        public float Length
        {
            get
            {
                return End.Position;
            }
        }
        /// <summary>
        /// Gets all the curve control points
        /// </summary>
        public Vector3[] Points
        {
            get
            {
                Vector3[] points = new Vector3[PositionCount];

                for (int i = 0; i < PositionCount; i++)
                {
                    points[i] = GetKey(i).Value;
                }

                return points;
            }
        }

        /// <summary>
        /// Sets the curve tangents for the current key, by previous and next keys
        /// </summary>
        /// <param name="prev">Previous key</param>
        /// <param name="curr">Current key</param>
        /// <param name="next">Next key</param>
        private static void SetCurveKeyTangent(ref CurveKey prev, ref CurveKey curr, ref CurveKey next)
        {
            float dt = next.Position - prev.Position;
            float dv = next.Value - prev.Value;
            if (MathF.Abs(dv) < float.Epsilon)
            {
                curr.TangentIn = 0;
                curr.TangentOut = 0;
            }
            else
            {
                curr.TangentIn = dv * (curr.Position - prev.Position) / dt;
                curr.TangentOut = dv * (next.Position - curr.Position) / dt;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Curve3D()
        {
            cX.PreLoop = CurveLoopType.Oscillate;
            cY.PreLoop = CurveLoopType.Oscillate;
            cZ.PreLoop = CurveLoopType.Oscillate;

            cX.PostLoop = CurveLoopType.Oscillate;
            cY.PostLoop = CurveLoopType.Oscillate;
            cZ.PostLoop = CurveLoopType.Oscillate;
        }

        /// <summary>
        /// Adds a new position to the curve
        /// </summary>
        /// <param name="position">Time position</param>
        /// <param name="vector">Position</param>
        public void AddPosition(float position, Vector3 vector)
        {
            cX.Keys.Add(new CurveKey(position, vector.X));
            cY.Keys.Add(new CurveKey(position, vector.Y));
            cZ.Keys.Add(new CurveKey(position, vector.Z));
        }
        /// <summary>
        /// Gets the curve key at specified index
        /// </summary>
        /// <param name="index">Curve index</param>
        /// <returns>Returns the key at specified index</returns>
        public Curve3DKey GetKey(int index)
        {
            var keyX = cX.Keys[index];
            var keyY = cY.Keys[index];
            var keyZ = cZ.Keys[index];

            return new Curve3DKey(
                keyX.Position,
                new Vector3(keyX.Value, keyY.Value, keyZ.Value),
                new Vector3(keyX.TangentIn, keyY.TangentIn, keyZ.TangentIn),
                new Vector3(keyX.TangentOut, keyY.TangentOut, keyZ.TangentOut),
                keyX.Continuity);
        }
        /// <summary>
        /// Gets the curve position at specified time
        /// </summary>
        /// <param name="time">Curve time</param>
        /// <returns>Returns the curve position at specified time</returns>
        public Vector3 GetPosition(float time)
        {
            return new Vector3(
                cX.Evaluate(time),
                cY.Evaluate(time),
                cZ.Evaluate(time));
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
        /// <param name="time">Curve time</param>
        /// <returns>Returns the next control point at specified time</returns>
        public Vector3 GetNextControlPoint(float time)
        {
            return new Vector3(
                cX.Evaluate(time),
                cY.Evaluate(time),
                cZ.Evaluate(time));
        }
        /// <summary>
        /// Sets the curve tangents
        /// </summary>
        public void SetTangents()
        {
            CurveKey prev;
            CurveKey curr;
            CurveKey next;
            int prevIndex;
            int nextIndex;
            for (int i = 0; i < cX.Keys.Count; i++)
            {
                prevIndex = i - 1;
                if (prevIndex < 0) prevIndex = i;

                nextIndex = i + 1;
                if (nextIndex == cX.Keys.Count) nextIndex = i;

                prev = cX.Keys[prevIndex];
                next = cX.Keys[nextIndex];
                curr = cX.Keys[i];
                SetCurveKeyTangent(ref prev, ref curr, ref next);
                cX.Keys[i] = curr;

                prev = cY.Keys[prevIndex];
                next = cY.Keys[nextIndex];
                curr = cY.Keys[i];
                SetCurveKeyTangent(ref prev, ref curr, ref next);
                cY.Keys[i] = curr;

                prev = cZ.Keys[prevIndex];
                next = cZ.Keys[nextIndex];
                curr = cZ.Keys[i];
                SetCurveKeyTangent(ref prev, ref curr, ref next);
                cZ.Keys[i] = curr;
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

            return [.. returnPath];
        }
    }
}
