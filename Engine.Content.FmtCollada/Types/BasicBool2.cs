using System;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class BasicBool2 : BasicBoolArray
    {
        public BasicBool2()
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
