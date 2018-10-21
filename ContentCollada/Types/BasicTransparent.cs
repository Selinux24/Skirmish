using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicTransparent
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("opaque")]
        public EnumOpaque Opaque { get; set; }

        public override string ToString()
        {
            return string.Format("SId: {0}; Value: {1};", this.SId, this.Opaque);
        }
    }
}
