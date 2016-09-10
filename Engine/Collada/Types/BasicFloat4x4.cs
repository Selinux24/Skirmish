using SharpDX;
using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat4x4 : BasicFloatArray
    {
        public BasicFloat4x4()
        {

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
