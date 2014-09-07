using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class GeographicLocation
    {
        [XmlElement("longitude")]
        public float Longitude { get; set; }
        [XmlElement("latitude")]
        public float Latitude { get; set; }
        [XmlElement("altitude")]
        public AltitudeModes Altitude { get; set; }
    }
}
