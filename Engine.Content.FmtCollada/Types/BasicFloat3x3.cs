using System;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat3X3 : BasicFloatArray
    {
        public BasicFloat3X3()
        {

        }

        public override string ToString()
        {
            if (this.Values != null && this.Values.Length == 9)
            {
                return string.Format(
                    "(M11:{0}, M12:{1}, M13:{2}, M21:{3}, M22:{4}, M23:{5}, M31:{6}, M32:{7}, M33:{8})",
                    this.Values[0], this.Values[1], this.Values[2],
                    this.Values[3], this.Values[4], this.Values[5],
                    this.Values[6], this.Values[7], this.Values[8]);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
