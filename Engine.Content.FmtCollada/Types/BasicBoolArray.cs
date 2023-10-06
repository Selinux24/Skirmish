using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicBoolArray
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
                Values = Collada.ConvertArray<bool>(value);
            }
        }
        [XmlIgnore]
        public bool[] Values { get; set; }
        [XmlIgnore]
        public bool this[int index]
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Count: {(Values != null ? Values.Length : 0)};";
        }
    }
}
