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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"SId: {SId}; Value: {Opaque};";
        }
    }
}
