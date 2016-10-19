using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat4 : BasicFloatArray
    {
        public BasicFloat4()
        {

        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 4)
            {
                return string.Format("({0}, {1}, {2}, {3})", this.Values[0], this.Values[1], this.Values[2], this.Values[3]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
