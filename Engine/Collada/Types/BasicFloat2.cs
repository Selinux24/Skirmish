using System;
using System.Xml.Serialization;
using SharpDX;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat2 : BasicFloatArray
    {
        public BasicFloat2()
        {

        }

        public BasicFloat2(float a, float b)
        {
            this.Values = new float[] { a, b };
        }

        public Vector2 ToVector2()
        {
            if (this.Values != null && this.Values.Length == 2)
            {
                return new Vector2(this.Values[0], this.Values[1]);
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", this.GetType()));
            }
        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 2)
            {
                return string.Format("({0}, {1})", this.Values[0], this.Values[1]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
