using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicBool4 : BasicBoolArray
    {
        public BasicBool4()
        {

        }

        public BasicBool4(bool a, bool b, bool c, bool d)
        {
            this.Values = new bool[] { a, b, c, d };
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
