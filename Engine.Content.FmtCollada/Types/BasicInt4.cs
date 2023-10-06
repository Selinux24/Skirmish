using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicInt4 : BasicIntArray
    {
        public BasicInt4()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Values != null && Values.Length == 4)
            {
                return $"({Values[0]}, {Values[1]}, {Values[2]}, {Values[3]})";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
