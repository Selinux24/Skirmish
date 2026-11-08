using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    [Serializable]
    public class Code
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
    }
}
