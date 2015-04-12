using System;
using SharpDX;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat3 : BasicFloatArray
    {
        public BasicFloat3()
        {

        }

        public BasicFloat3(float a, float b, float c)
        {
            this.Values = new float[] { a, b, c };
        }

        public Vector3 ToVector3()
        {
            if (this.Values != null && this.Values.Length == 3)
            {
                return new Vector3(this.Values[0], this.Values[1], this.Values[2]);
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", this.GetType()));
            }
        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 3)
            {
                return string.Format("({0}, {1}, {2})", this.Values[0], this.Values[1], this.Values[2]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
