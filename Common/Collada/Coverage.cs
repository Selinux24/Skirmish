using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    using Common.Collada.Types;

    [Serializable]
    public class Coverage
    {
        [XmlElement("geographic_location")]
        public GeographicLocation GeographicLocation { get; set; }
    }
}
