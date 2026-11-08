using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class Unit
    {
        [XmlAttribute("meter")]
        public float Meter { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Name: {Name}; Meter: {Meter}";
        }
    }
}
