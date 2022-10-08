using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicIntArray
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlText]
        public string Text
        {
            get
            {
                return Collada.ConvertArrayToString(this.Values);
            }
            set
            {
                this.Values = Collada.ConvertArray<int>(value);
            }
        }
        [XmlIgnore]
        public int[] Values { get; set; }
        [XmlIgnore]
        public int this[int index]
        {
            get
            {
                return this.Values[index];
            }
            set
            {
                this.Values[index] = value;
            }
        }

        public int Sum()
        {
            int res = 0;

            foreach (int v in this.Values)
            {
                res += v;
            }

            return res;
        }

        public override string ToString()
        {
            return string.Format("Count: {0};", this.Values != null ? this.Values.Length : 0);
        }
    }
}
