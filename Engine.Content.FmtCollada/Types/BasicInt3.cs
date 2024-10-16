using System;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class BasicInt3 : BasicIntArray
    {
        public BasicInt3()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Values != null && Values.Length == 3)
            {
                return $"({Values[0]}, {Values[1]}, {Values[2]})";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
