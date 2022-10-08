using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat2X2 : BasicFloatArray
    {
        public BasicFloat2X2()
        {

        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 4)
            {
                return string.Format("(M11:{0}, M12:{1}, M21:{2}, M22:{3})", this.Values[0], this.Values[1], this.Values[2], this.Values[3]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
