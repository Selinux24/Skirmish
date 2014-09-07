using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class SurfaceInitFromType
    {
        [XmlAttributeAttribute("mip")]
        [DefaultValueAttribute(typeof(uint), "0")]
        public uint Mip { get; set; }
        [XmlAttributeAttribute("slice")]
        [DefaultValueAttribute(typeof(uint), "0")]
        public uint Slice { get; set; }
        [XmlAttributeAttribute("face")]
        [DefaultValueAttribute(SurfaceFaceEnum.PositiveX)]
        public SurfaceFaceEnum Face { get; set; }
        [XmlTextAttribute(DataType = "IDREF")]
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.Value);
        }
    }
}
