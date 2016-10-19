using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloatArray
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlText]
        public string Text
        {
            get
            {
                return COLLADA.ConvertArrayToString(this.Values);
            }
            set
            {
                this.Values = COLLADA.ConvertArray<float>(value);
            }
        }
        [XmlIgnore]
        public float[] Values { get; set; }
        [XmlIgnore]
        public float this[int index]
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

        public float Sum()
        {
            float res = 0;

            foreach (float v in this.Values)
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
