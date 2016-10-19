using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat3x3 : BasicFloatArray
    {
        public BasicFloat3x3()
        {

        }

        public BasicFloat3x3(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33)
        {
            this.Values = new float[] { m11, m12, m13, m21, m22, m23, m31, m32, m33 };
        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 9)
            {
                return string.Format(
                    "(M11:{0}, M12:{1}, M13:{2}, M21:{3}, M22:{4}, M23:{5}, M31:{6}, M32:{7}, M33:{8})",
                    this.Values[0], this.Values[1], this.Values[2],
                    this.Values[3], this.Values[4], this.Values[5],
                    this.Values[6], this.Values[7], this.Values[8]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
