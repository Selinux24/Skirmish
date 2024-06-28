using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicInt2 : BasicIntArray
    {
        public BasicInt2()
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
