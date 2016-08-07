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

        public int KeyCount
        {
            get
            {
                return this.cX.Keys.Count;
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

        public Vector3 GetKey(int index)
        {
            return new Vector3(
                this.cX.Keys[index].Value,
                this.cY.Keys[index].Value,
                this.cZ.Keys[index].Value);
        }

        public float Length
        {
            get
            {
                float length = 0;

                for (int i = 1; i < this.KeyCount; i++)
                {
                    length += Vector3.DistanceSquared(this.GetKey(i - 1), this.GetKey(i));
                }

                return (float)Math.Sqrt(length);
            }
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
