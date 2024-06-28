using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    [Serializable]
    public class SurfaceInitFrom
    {
        [XmlAttribute("mip")]
        public int Mip { get; set; }
        [XmlAttribute("slice")]
        public int Slice { get; set; }
        [XmlAttribute("face")]
        public EnumSurfaceFaces Face { get; set; }
        [XmlText(DataType = "IDREF")]
        public string Value { get; set; }

        public SurfaceInitFrom()
        {
            Mip = 0;
            Slice = 0;
            Face = EnumSurfaceFaces.PositiveX;
        }
    }
}
