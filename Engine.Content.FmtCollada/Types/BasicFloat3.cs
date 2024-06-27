using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat3 : BasicFloatArray
    {
        public BasicFloat3()
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
