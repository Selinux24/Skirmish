using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class EvaluateScene
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("render", typeof(Render))]
        public Render[] Renders { get; set; }
    }
}
