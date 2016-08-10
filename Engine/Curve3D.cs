using System;
using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// 
    /// </summary>
    public class Curve3D : ICurve
    {
        private Curve cX = new Curve();
        private Curve cY = new Curve();
        private Curve cZ = new Curve();

        /// <summary>
        /// Returns <c>true</c> if this curve is constant (has zero or one points); <c>false</c> otherwise.
        /// </summary>
        public bool IsConstant
        {
            get { return this.KeyCount <= 1; }
        }
        /// <summary>
        /// Defines how to handle weighting values that are greater than the last control point in the curve.
        /// </summary>
        public CurveLoopType PostLoop
        {
            get
            {
                return this.cX.PostLoop;
            }
            set
            {
                this.cX.PostLoop = value;
                this.cY.PostLoop = value;
                this.cZ.PostLoop = value;
            }
        }
        /// <summary>
        /// Defines how to handle weighting values that are less than the first control point in the curve.
        /// </summary>
        public CurveLoopType PreLoop
        {
            get
            {
                return this.cX.PreLoop;
            }
            set
            {
                this.cX.PreLoop = value;
                this.cY.PreLoop = value;
                this.cZ.PreLoop = value;
            }
        }
        public int KeyCount
        {
            get
            {
                return this.cX.Keys.Count;
            }
        }
        public Curve3DKey Start
        {
            get
            {
                return this.GetKey(0);
            }
        }
        public Curve3DKey[] Keys
        {
            get
            {
                Curve3DKey[] keys = new Curve3DKey[this.KeyCount];

                for (int i = 0; i < this.KeyCount; i++)
                {
                    keys[i] = this.GetKey(i);
                }

                return keys;
            }
        }
        public Curve3DKey End
        {
            get
            {
                return this.GetKey(this.KeyCount - 1);
            }
        }
        public float Length
        {
            get
            {
                return this.End.Position;
            }
        }
        public Vector3[] Points
        {
            get
            {
                Vector3[] points = new Vector3[this.KeyCount];

                for (int i = 0; i < this.KeyCount; i++)
                {
                    points[i] = this.GetKey(i).Value;
                }

                return points;
            }
        }

        public Curve3D()
        {
            this.cX.PreLoop = CurveLoopType.Oscillate;
            this.cY.PreLoop = CurveLoopType.Oscillate;
            this.cZ.PreLoop = CurveLoopType.Oscillate;

            this.cX.PostLoop = CurveLoopType.Oscillate;
            this.cY.PostLoop = CurveLoopType.Oscillate;
            this.cZ.PostLoop = CurveLoopType.Oscillate;
        }

        public void AddPosition(float position, Vector3 vector)
        {
            this.cX.Keys.Add(new CurveKey(position, vector.X));
            this.cY.Keys.Add(new CurveKey(position, vector.Y));
            this.cZ.Keys.Add(new CurveKey(position, vector.Z));
        }

        public Curve3DKey GetKey(int index)
        {
            var keyX = this.cX.Keys[index];
            var keyY = this.cY.Keys[index];
            var keyZ = this.cZ.Keys[index];

            return new Curve3DKey(
                keyX.Position,
                new Vector3(keyX.Value, keyY.Value, keyZ.Value),
                new Vector3(keyX.TangentIn, keyY.TangentIn, keyZ.TangentIn),
                new Vector3(keyX.TangentOut, keyY.TangentOut, keyZ.TangentOut),
                keyX.Continuity);
        }

        public Vector3 GetPosition(float time)
        {
            return new Vector3(
                this.cX.Evaluate(time),
                this.cY.Evaluate(time),
                this.cZ.Evaluate(time));
        }

        public void SetTangents()
        {
            CurveKey prev;
            CurveKey curr;
            CurveKey next;
            int prevIndex;
            int nextIndex;
            for (int i = 0; i < this.cX.Keys.Count; i++)
            {
                prevIndex = i - 1;
                if (prevIndex < 0) prevIndex = i;

                nextIndex = i + 1;
                if (nextIndex == this.cX.Keys.Count) nextIndex = i;

                prev = this.cX.Keys[prevIndex];
                next = this.cX.Keys[nextIndex];
                curr = this.cX.Keys[i];
                SetCurveKeyTangent(ref prev, ref curr, ref next);
                this.cX.Keys[i] = curr;

                prev = this.cY.Keys[prevIndex];
                next = this.cY.Keys[nextIndex];
                curr = this.cY.Keys[i];
                SetCurveKeyTangent(ref prev, ref curr, ref next);
                this.cY.Keys[i] = curr;

                prev = this.cZ.Keys[prevIndex];
                next = this.cZ.Keys[nextIndex];
                curr = this.cZ.Keys[i];
                SetCurveKeyTangent(ref prev, ref curr, ref next);
                this.cZ.Keys[i] = curr;
            }

        }

        private void SetCurveKeyTangent(ref CurveKey prev, ref CurveKey curr, ref CurveKey next)
        {
            float dt = next.Position - prev.Position;
            float dv = next.Value - prev.Value;
            if (Math.Abs(dv) < float.Epsilon)
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
    }
}
