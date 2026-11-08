using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
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
                return Collada.ConvertArrayToString(Values);
            }
            set
            {
                Values = Collada.ConvertArray<int>(value);
            }
        }
        [XmlIgnore]
        public int[] Values { get; set; }
        [XmlIgnore]
        public int this[int index]
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

        public int Sum()
        {
            int res = 0;

            foreach (int v in Values)
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
