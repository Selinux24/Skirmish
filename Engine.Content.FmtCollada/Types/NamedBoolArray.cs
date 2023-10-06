using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class NamedBoolArray : NamedArray
    {
        [XmlText]
        public string Text
        {
            get
            {
                return Collada.ConvertToString(Values);
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
            return base.ToString() + $" Values: {Text};";
        }
    }
}
