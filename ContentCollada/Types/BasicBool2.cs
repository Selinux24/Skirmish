using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicBool2 : BasicBoolArray
    {
        public BasicBool2()
        {

        }

        public BasicBool2(bool a, bool b)
        {
            this.Values = new bool[] { a, b };
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
