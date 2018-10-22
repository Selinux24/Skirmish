using System;

namespace Engine
{
    /// <summary>
    /// Contains a collection of <see cref="CurveKey"/> points in 2D space and provides methods for evaluating features of the curve they define.
    /// </summary>
    public class Curve
    {
        /// <summary>
        /// Returns <c>true</c> if this curve is constant (has zero or one points); <c>false</c> otherwise.
        /// </summary>
        public bool IsConstant
        {
            get { return this.Keys.Count <= 1; }
        }
        /// <summary>
        /// The collection of curve keys.
        /// </summary>
        public CurveKeyCollection Keys { get; private set; }
        /// <summary>
        /// Defines how to handle weighting values that are greater than the last control point in the curve.
        /// </summary>
        public CurveLoopType PostLoop { get; set; }
        /// <summary>
        /// Defines how to handle weighting values that are less than the first control point in the curve.
        /// </summary>
        public CurveLoopType PreLoop { get; set; }

        /// <summary>
        /// Constructs a curve.
        /// </summary>
        public Curve()
        {
            this.Keys = new CurveKeyCollection();
        }

        /// <summary>
        /// Evaluates the value at a position of this <see cref="Curve"/>.
        /// </summary>
        /// <param name="position">The position on this <see cref="Curve"/>.</param>
        /// <returns>Returns the value at the position on this <see cref="Curve"/>.</returns>
        public float Evaluate(float position)
        {
            if (this.Keys.Count == 0)
            {
                return 0f;
            }

            if (this.Keys.Count == 1)
            {
                return this.Keys[0].Value;
            }

            if (this.PreLoop == CurveLoopType.Constant)
            {
                return EvaluateConstant(this.Keys, position);
            }
            else if (this.PreLoop == CurveLoopType.Linear)
            {
                return EvaluateLinear(this.Keys, position);
            }
            else if (this.PreLoop == CurveLoopType.Cycle)
            {
                return EvaluateCycle(this.Keys, position);
            }
            else if (this.PreLoop == CurveLoopType.CycleOffset)
            {
                return EvaluateCycleOffset(this.Keys, position);
            }
            else if (this.PreLoop == CurveLoopType.Oscillate)
            {
                return EvaluateOscillate(this.Keys, position);
            }
            else
            {
                throw new EngineException($"Bad curve loop type: {this.PreLoop}");
            }
        }
        /// <summary>
        /// Gets the cycle number
        /// </summary>
        /// <param name="first">First key</param>
        /// <param name="last">Last key</param>
        /// <param name="position">Position to test</param>
        /// <returns>Returns the cycle number</returns>
        private static int GetNumberOfCycle(CurveKey first, CurveKey last, float position)
        {
            float cycle = (position - first.Position) / (last.Position - first.Position);
            if (cycle < 0f)
            {
                cycle--;
            }

            return (int)cycle;
        }
        /// <summary>
        /// Gets the curve position
        /// </summary>
        /// <param name="keys">Key collection</param>
        /// <param name="position">Position to test</param>
        /// <returns>Returns the curve position</returns>
        private static float GetCurvePosition(CurveKeyCollection keys, float position)
        {
            //only for position in curve
            CurveKey prev = keys[0];
            CurveKey next;

            for (int i = 1; i < keys.Count; ++i)
            {
                next = keys[i];
                if (next.Position >= position)
                {
                    if (prev.Continuity == CurveContinuity.Step)
                    {
                        if (position >= 1f)
                        {
                            return next.Value;
                        }

                        return prev.Value;
                    }

                    //http://en.wikipedia.org/wiki/Cubic_Hermite_spline
                    //P(t) = (2*t^3 - 3t^2 + 1)*P0 + (t^3 - 2t^2 + t)m0 + (-2t^3 + 3t^2)P1 + (t^3-t^2)m1
                    //with P0.value = prev.value , m0 = prev.tangentOut, P1= next.value, m1 = next.TangentIn
                    float t = (position - prev.Position) / (next.Position - prev.Position);//to have t in [0,1]
                    float ts = t * t;
                    float tss = ts * t;
                    return (2 * tss - 3 * ts + 1f) * prev.Value + (tss - 2 * ts + t) * prev.TangentOut + (3 * ts - 2 * tss) * next.Value + (tss - ts) * next.TangentIn;
                }
                prev = next;
            }

            return 0f;
        }
        /// <summary>
        /// Evaluates the value at a position of this <see cref="Curve"/>.
        /// </summary>
        /// <param name="keys">Curve keys</param>
        /// <param name="position">Position to test</param>
        /// <returns>Returns the value at the position on this <see cref="Curve"/>.</returns>
        private static float EvaluateConstant(CurveKeyCollection keys, float position)
        {
            var first = keys[0];
            var last = keys[keys.Count - 1];

            if (position < first.Position)
            {
                //constant
                return first.Value;
            }
            else if (position > last.Position)
            {
                //constant
                return last.Value;
            }

            return GetCurvePosition(keys, position);
        }
        /// <summary>
        /// Evaluates the value at a position of this <see cref="Curve"/>.
        /// </summary>
        /// <param name="keys">Curve keys</param>
        /// <param name="position">Position to test</param>
        /// <returns>Returns the value at the position on this <see cref="Curve"/>.</returns>
        private static float EvaluateLinear(CurveKeyCollection keys, float position)
        {
            var first = keys[0];
            var last = keys[keys.Count - 1];

            if (position < first.Position)
            {
                // linear y = a*x +b with a tangeant of last point
                return first.Value - first.TangentIn * (first.Position - position);
            }
            else if (position > last.Position)
            {
                // linear y = a*x +b with a tangeant of last point
                return last.Value + first.TangentOut * (position - last.Position);
            }

            return GetCurvePosition(keys, position);
        }
        /// <summary>
        /// Evaluates the value at a position of this <see cref="Curve"/>.
        /// </summary>
        /// <param name="keys">Curve keys</param>
        /// <param name="position">Position to test</param>
        /// <returns>Returns the value at the position on this <see cref="Curve"/>.</returns>
        private static float EvaluateCycle(CurveKeyCollection keys, float position)
        {
            var first = keys[0];
            var last = keys[keys.Count - 1];

            if (position != first.Position)
            {
                //start -> end / start -> end
                int cycle = GetNumberOfCycle(first, last, position);
                float virtualPos = position - (cycle * (last.Position - first.Position));
                return GetCurvePosition(keys, virtualPos);
            }

            //in curve
            return GetCurvePosition(keys, position);
        }
        /// <summary>
        /// Evaluates the value at a position of this <see cref="Curve"/>.
        /// </summary>
        /// <param name="keys">Curve keys</param>
        /// <param name="position">Position to test</param>
        /// <returns>Returns the value at the position on this <see cref="Curve"/>.</returns>
        private static float EvaluateCycleOffset(CurveKeyCollection keys, float position)
        {
            var first = keys[0];
            var last = keys[keys.Count - 1];

            if (position != first.Position)
            {
                //make the curve continue (with no step) so must up the curve each cycle of delta(value)
                int cycle = GetNumberOfCycle(first, last, position);
                float virtualPos = position - (cycle * (last.Position - first.Position));
                return (GetCurvePosition(keys, virtualPos) + cycle * (last.Value - first.Value));
            }

            //in curve
            return GetCurvePosition(keys, position);
        }
        /// <summary>
        /// Evaluates the value at a position of this <see cref="Curve"/>.
        /// </summary>
        /// <param name="keys">Curve keys</param>
        /// <param name="position">Position to test</param>
        /// <returns>Returns the value at the position on this <see cref="Curve"/>.</returns>
        private static float EvaluateOscillate(CurveKeyCollection keys, float position)
        {
            var first = keys[0];
            var last = keys[keys.Count - 1];

            if (position != first.Position)
            {
                //go back on curve from end and target start 
                // start-> end / end -> start
                int cycle = GetNumberOfCycle(first, last, position);
                float virtualPos;
                if (0 == cycle % 2f)
                {
                    //if pair
                    virtualPos = position - (cycle * (last.Position - first.Position));
                }
                else
                {
                    virtualPos = last.Position - position + first.Position + (cycle * (last.Position - first.Position));
                }
                return GetCurvePosition(keys, virtualPos);
            }

            //in curve
            return GetCurvePosition(keys, position);
        }

        /// <summary>
        /// Computes tangents for all keys in the collection.
        /// </summary>
        /// <param name="tangentType">The tangent type for both in and out.</param>
        public void ComputeTangents(CurveTangent tangentType)
        {
            this.ComputeTangents(tangentType, tangentType);
        }
        /// <summary>
        /// Computes tangents for all keys in the collection.
        /// </summary>
        /// <param name="tangentInType">The tangent in-type. <see cref="CurveKey.TangentIn"/> for more details.</param>
        /// <param name="tangentOutType">The tangent out-type. <see cref="CurveKey.TangentOut"/> for more details.</param>
        public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            for (var i = 0; i < Keys.Count; ++i)
            {
                this.ComputeTangent(i, tangentInType, tangentOutType);
            }
        }
        /// <summary>
        /// Computes tangent for the specific key in the collection.
        /// </summary>
        /// <param name="keyIndex">The index of a key in the collection.</param>
        /// <param name="tangentType">The tangent type for both in and out.</param>
        public void ComputeTangent(int keyIndex, CurveTangent tangentType)
        {
            this.ComputeTangent(keyIndex, tangentType, tangentType);
        }
        /// <summary>
        /// Computes tangent for the specific key in the collection.
        /// </summary>
        /// <param name="keyIndex">The index of key in the collection.</param>
        /// <param name="tangentInType">The tangent in-type. <see cref="CurveKey.TangentIn"/> for more details.</param>
        /// <param name="tangentOutType">The tangent out-type. <see cref="CurveKey.TangentOut"/> for more details.</param>
        public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            // See http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.curvetangent.aspx

            var key = this.Keys[keyIndex];

            float p0, p, p1;
            p0 = p = p1 = key.Position;

            float v0, v, v1;
            v0 = v = v1 = key.Value;

            if (keyIndex > 0)
            {
                p0 = this.Keys[keyIndex - 1].Position;
                v0 = this.Keys[keyIndex - 1].Value;
            }

            if (keyIndex < this.Keys.Count - 1)
            {
                p1 = this.Keys[keyIndex + 1].Position;
                v1 = this.Keys[keyIndex + 1].Value;
            }

            if (tangentInType == CurveTangent.Flat)
            {
                key.TangentIn = 0;
            }
            else if (tangentInType == CurveTangent.Linear)
            {
                key.TangentIn = v - v0;
            }
            else if (tangentInType == CurveTangent.Smooth)
            {
                var pn = p1 - p0;
                if (Math.Abs(pn) < float.Epsilon)
                {
                    key.TangentIn = 0;
                }
                else
                {
                    key.TangentIn = (v1 - v0) * ((p - p0) / pn);
                }
            }

            if (tangentOutType == CurveTangent.Flat)
            {
                key.TangentOut = 0;
            }
            else if (tangentOutType == CurveTangent.Linear)
            {
                key.TangentOut = v1 - v;
            }
            else if (tangentOutType == CurveTangent.Smooth)
            {
                var pn = p1 - p0;
                if (Math.Abs(pn) < float.Epsilon)
                {
                    key.TangentOut = 0;
                }
                else
                {
                    key.TangentOut = (v1 - v0) * ((p1 - p) / pn);
                }
            }
        }
    }
}
