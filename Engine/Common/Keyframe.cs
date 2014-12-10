using System;
using SharpDX;

namespace Engine.Common
{
    [Serializable]
    public struct Keyframe
    {
        private Matrix transform;

        public float Time;
        public Matrix Transform
        {
            get
            {
                return this.transform;
            }
            set
            {
                Matrix trn = value;

                Vector3 location;
                Quaternion rotation;
                Vector3 scale;
                if (trn.Decompose(out scale, out rotation, out location))
                {
                    this.transform = trn;
                    this.Location = location;
                    this.Rotation = rotation;
                    this.Scale = scale;
                }
                else
                {
                    throw new Exception("Bad transform");
                }
            }
        }
        public Vector3 Location { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Scale { get; private set; }
        public string Interpolation;

        public static Matrix Interpolate(Keyframe from, Keyframe to, float amount)
        {
            return
                Matrix.Scaling(Vector3.Lerp(from.Scale, to.Scale, amount)) *
                Matrix.RotationQuaternion(Quaternion.Slerp(from.Rotation, to.Rotation, amount)) *
                Matrix.Translation(Vector3.Lerp(from.Location, to.Location, amount));
        }

        public override string ToString()
        {
            return string.Format("Time: {0}", this.Time);
        }
    }
}
