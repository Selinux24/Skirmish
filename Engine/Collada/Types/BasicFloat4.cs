using System;
using SharpDX;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat4 : BasicFloatArray
    {
        public BasicFloat4()
        {

        }

        public BasicFloat4(float a, float b, float c, float d)
        {
            this.Values = new float[] { a, b, c, d };
        }

        public Vector4 ToVector4()
        {
            if (this.Values != null && this.Values.Length == 4)
            {
                return new Vector4(this.Values[0], this.Values[1], this.Values[2], this.Values[3]);
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", this.GetType()));
            }
        }

        public Quaternion ToQuaternion()
        {
            if (this.Values != null && this.Values.Length == 4)
            {
                return new Quaternion(this.Values[0], this.Values[1], this.Values[2], this.Values[3]);
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", this.GetType()));
            }
        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 4)
            {
                return string.Format("({0}, {1}, {2}, {3})", this.Values[0], this.Values[1], this.Values[2], this.Values[3]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
