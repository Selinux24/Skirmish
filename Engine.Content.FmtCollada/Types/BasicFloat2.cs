using System;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class BasicFloat2 : BasicFloatArray
    {
        public BasicFloat2()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Values != null && Values.Length == 2)
            {
                return $"({Values[0]}, {Values[1]})";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
