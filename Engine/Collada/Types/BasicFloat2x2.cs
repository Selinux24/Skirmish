using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat2x2 : BasicFloatArray
    {
        public BasicFloat2x2()
        {

        }

        public BasicFloat2x2(float m11, float m12, float m21, float m22)
        {
            this.Values = new float[] { m11, m12, m21, m22 };
        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 4)
            {
                return string.Format("(M11:{0}, M12:{1}, M21:{2}, M22:{3})", this.Values[0], this.Values[1], this.Values[2], this.Values[3]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
