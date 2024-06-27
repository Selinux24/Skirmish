using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Asset
    {
        [XmlElement("contributor", typeof(Contributor))]
        public Contributor Contributor { get; set; }
        [XmlElement("created")]
        public string Created { get; set; }
        [XmlElement("keywords")]
        public string Keywords { get; set; }
        [XmlElement("modified")]
        public string Modified { get; set; }
        [XmlElement("revision")]
        public string Revision { get; set; }
        [XmlElement("subject")]
        public string Subject { get; set; }
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("unit", typeof(Unit))]
        public Unit Unit { get; set; }
        [XmlElement("up_axis", typeof(EnumAxis))]
        public EnumAxis UpAxis { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Title: {Title}; {Contributor}";
        }
    }
}
