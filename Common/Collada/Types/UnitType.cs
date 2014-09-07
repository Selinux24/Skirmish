using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class UnitType
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("meter")]
        public float Meter { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}; {1} meter;", this.Name, this.Meter);
        }
    }
}
