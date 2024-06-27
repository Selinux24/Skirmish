using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class NamedIdRefArray : NamedArray
    {
        [XmlText(DataType = "IDREFS")]
        public string Value
        {
            get
            {
                return Collada.ConvertArrayToString(Values);
            }
            set
            {
                Values = Collada.ConvertArray<string>(value);
            }
        }
        [XmlIgnore]
        public string[] Values { get; set; }
        [XmlIgnore]
        public string this[int index]
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
            return base.ToString() + $" Value: {Value};";
        }
    }
}
