using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    [Serializable]
    public class Include
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("url")]
        public string Url { get; set; }
    }
}
