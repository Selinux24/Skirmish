using System;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class BasicColor : BasicFloatArray
    {
        public BasicColor()
        {

        }

        public BasicColor(float r, float g, float b)
        {
            Values = [r, g, b];
        }

        public BasicColor(float r, float g, float b, float a)
        {
            Values = [r, g, b, a];
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Values != null && Values.Length == 3)
            {
                return $"(R:{Values[0]}, G:{Values[1]}, B:{Values[2]})";
            }
            else if (Values != null && Values.Length == 4)
            {
                return $"(R:{Values[0]}, G:{Values[1]}, B:{Values[2]}, A{Values[3]})";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
