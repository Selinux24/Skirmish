using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Camera : NamedNode
    {
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("optics", typeof(Optics))]
        public Optics Optics { get; set; }
        [XmlElement("imager", typeof(Imager))]
        public Imager Imager { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public override string ToString()
        {
            return "Camera; " + base.ToString();
        }
    }
}
