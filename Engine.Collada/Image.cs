using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Image : NamedNode
    {
        [XmlAttribute("format")]
        public string Format { get; set; }
        [XmlAttribute("height")]
        public int Height { get; set; }
        [XmlAttribute("width")]
        public int Width { get; set; }
        [XmlAttribute("depth")]
        public int Depth { get; set; }
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("data")]
        public object Data { get; set; }
        [XmlElement("init_from")]
        public string InitFrom { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public override string ToString()
        {
            return string.Format("Image; Format: {0}; Height: {1}; Width: {2}; Depth: {3}; ", this.Format, this.Height, this.Width, this.Depth) + base.ToString();
        }
    }
}
