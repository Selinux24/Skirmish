using System;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class BasicFloat3X3 : BasicFloatArray
    {
        public BasicFloat3X3()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Values != null && Values.Length == 9)
            {
                return $"(M11:{Values[0]}, M12:{Values[1]}, M13:{Values[2]}, M21:{Values[3]}, M22:{Values[4]}, M23:{Values[5]}, M31:{Values[6]}, M32:{Values[7]}, M33:{Values[8]})";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
