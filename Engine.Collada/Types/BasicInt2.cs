using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicInt2 : BasicIntArray
    {
        public BasicInt2()
        {

        }

        public BasicInt2(int a, int b)
        {
            this.Values = new int[] { a, b };
        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 2)
            {
                return string.Format("({0}, {1})", this.Values[0], this.Values[1]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
