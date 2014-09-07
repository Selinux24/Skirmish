using System;
using System.Xml.Serialization;
using SharpDX;

namespace Common.Collada.Types
{
    [Serializable]
    public class Vector3Type
    {
        [XmlText]
        public string StringValue
        {
            get { return Dae.Convert(this.Value); }
            set { this.Value = Dae.ConvertVector3(value); }
        }
        [XmlIgnore]
        public Vector3 Value { get; set; }
    }
}
