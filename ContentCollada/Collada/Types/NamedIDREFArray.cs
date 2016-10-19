using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class NamedIDREFArray : NamedArray
    {
        [XmlText(DataType = "IDREFS")]
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("Value: {0}; ", this.Value) + base.ToString();
        }
    }
}
