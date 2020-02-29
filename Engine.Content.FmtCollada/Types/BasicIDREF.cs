using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
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

        public override string ToString()
        {
            return string.Format("{0}", this.Reference);
        }
    }
}
