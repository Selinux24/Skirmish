using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat4X4 : BasicFloatArray
    {
        public BasicFloat4X4()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Values != null && Values.Length == 16)
            {
                return string.Format("(M11:{0}, M12:{1}, M13:{2}, M14:{3}, M21:{4}, M22:{5}, M23:{6}, M24:{7}, M31:{8}, M32:{9}, M33:{10}, M34:{11}, M41:{12}, M42:{13}, M43:{14}, M44:{15})",
                    Values[0], Values[1], Values[2], Values[3],
                    Values[4], Values[5], Values[6], Values[7],
                    Values[8], Values[9], Values[10], Values[11],
                    Values[12], Values[13], Values[14], Values[15]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
