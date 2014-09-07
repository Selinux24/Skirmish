using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class FloatArrayType
    {
        [XmlText]
        public string StringValue
        {
            get { return Dae.Convert(this.Value); }
            set { this.Value = Dae.ConvertFloatArray(value); }
        }
        [XmlIgnore]
        public List<float> Value { get; set; }
    }
}
