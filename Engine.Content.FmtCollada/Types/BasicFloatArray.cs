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
                return Collada.ConvertArrayToString(Values);
            }
            set
            {
                Values = Collada.ConvertArray<float>(value);
            }
        }
        [XmlIgnore]
        public float[] Values { get; set; }
        [XmlIgnore]
        public float this[int index]
        {
            get
            {
                return Values[index];
            }
            set
            {
                Values[index] = value;
            }
        }

        public float Sum()
        {
            float res = 0;

            foreach (float v in Values)
            {
                res += v;
            }

            return res;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Count: {(Values != null ? Values.Length : 0)};";
        }
    }
}
