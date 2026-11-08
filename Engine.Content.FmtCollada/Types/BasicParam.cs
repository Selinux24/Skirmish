using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class BasicParam
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("ref")]
        public string Reference { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Reference: {Reference};";
        }
    }
}
