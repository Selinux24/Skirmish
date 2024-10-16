using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class BasicIdRef
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("@ref", DataType = "IDREF")]
        public string Reference { get; set; }

        public BasicIdRef()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Reference}";
        }
    }
}
