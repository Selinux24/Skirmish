using System;
using SharpDX;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicColor : BasicFloatArray
    {
        public BasicColor()
        {

        }

        public BasicColor(float r, float g, float b)
        {
            this.Values = new float[] { r, g, b };
        }

        public BasicColor(float r, float g, float b, float a)
        {
            this.Values = new float[] { r, g, b, a };
        }

        public Color4 ToColor4()
        {
            if (this.Values != null && this.Values.Length == 3)
            {
                return new Color4(this.Values[0], this.Values[1], this.Values[2], 1f);
            }
            else if (this.Values != null && this.Values.Length == 4)
            {
                return new Color4(this.Values[0], this.Values[1], this.Values[2], this.Values[3]);
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
                return string.Format("(R:{0}, G:{1}, B:{2})", this.Values[0], this.Values[1], this.Values[2]);
            }
            else if (this.Values != null && this.Values.Length == 4)
            {
                return string.Format("(R:{0}, G:{1}, B:{2}, A{3})", this.Values[0], this.Values[1], this.Values[2], this.Values[3]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
