using System;
using System.Xml.Serialization;

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
