using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class Unit
    {
        [XmlAttribute("meter")]
        public float Meter { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}; Meter: {1}", this.Name, this.Meter);
        }
    }
}
