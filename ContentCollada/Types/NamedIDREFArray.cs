using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class NamedIDREFArray : NamedArray
    {
        [XmlText(DataType = "IDREFS")]
        public string Value
        {
            get
            {
                return Collada.ConvertArrayToString(this.Values);
            }
            set
            {
                this.Values = Collada.ConvertArray<string>(value);
            }
        }
        [XmlIgnore]
        public string[] Values { get; set; }
        [XmlIgnore]
        public string this[int index]
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

        public override string ToString()
        {
            return string.Format("Value: {0}; ", this.Value) + base.ToString();
        }
    }
}
