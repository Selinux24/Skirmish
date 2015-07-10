using System;
using SharpDX;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat4x4 : BasicFloatArray
    {
        public BasicFloat4x4()
        {

        }

        public BasicFloat4x4(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            this.Values = new float[] 
            { 
                m11, m12, m13, m14, 
                m21, m22, m23, m24,
                m31, m32, m33, m34,
                m41, m42, m43, m44,
            };
        }

        public Matrix ToMatrix()
        {
            if (this.Values != null && this.Values.Length == 16)
            {
                Matrix m = new Matrix()
                {
                    M11 = this.Values[0],
                    M12 = this.Values[1],
                    M13 = this.Values[2],
                    M14 = this.Values[3],

                    M21 = this.Values[4],
                    M22 = this.Values[5],
                    M23 = this.Values[6],
                    M24 = this.Values[7],

                    M31 = this.Values[8],
                    M32 = this.Values[9],
                    M33 = this.Values[10],
                    M34 = this.Values[11],

                    M41 = this.Values[12],
                    M42 = this.Values[13],
                    M43 = this.Values[14],
                    M44 = this.Values[15],
                };

                return m;
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", this.GetType()));
            }
        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 16)
            {
                return string.Format("(M11:{0}, M12:{1}, M13:{2}, M14:{3}, M21:{4}, M22:{5}, M23:{6}, M24:{7}, M31:{8}, M32:{9}, M33:{10}, M34:{11}, M41:{12}, M42:{13}, M43:{14}, M44:{15})",
                    this.Values[0], this.Values[1], this.Values[2], this.Values[3],
                    this.Values[4], this.Values[5], this.Values[6], this.Values[7],
                    this.Values[8], this.Values[9], this.Values[10], this.Values[11],
                    this.Values[12], this.Values[13], this.Values[14], this.Values[15]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
