using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat2X2 : BasicFloatArray
    {
        public BasicFloat2X2()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Values != null && Values.Length == 4)
            {
                return $"(M11:{Values[0]}, M12:{Values[1]}, M21:{Values[2]}, M22:{Values[3]})";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
