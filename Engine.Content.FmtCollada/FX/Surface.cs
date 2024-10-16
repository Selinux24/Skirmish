﻿using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class Surface
    {
        [XmlAttribute("type")]
        public EnumSurfaceTypes Type { get; set; }
        [XmlElement("init_as_null")]
        public object InitAsNull { get; set; }
        [XmlElement("init_as_target")]
        public object InitAsTarget { get; set; }
        [XmlElement("init_cube", typeof(SurfaceInitCube))]
        public SurfaceInitCube InitCube { get; set; }
        [XmlElement("init_volume", typeof(SurfaceInitVolume))]
        public SurfaceInitVolume InitVolume { get; set; }
        [XmlElement("init_planar", typeof(SurfaceInitPlanar))]
        public SurfaceInitPlanar InitPlanar { get; set; }
        [XmlElement("init_from", typeof(SurfaceInitFrom))]
        public SurfaceInitFrom InitFrom { get; set; }
        [XmlElement("format")]
        public string Format { get; set; }
        [XmlElement("format_hint", typeof(SurfaceFormatHint))]
        public SurfaceFormatHint FormatHint { get; set; }
        [XmlElement("size", typeof(BasicInt3))]
        public BasicInt3 Size { get; set; }
        [XmlElement("viewport_ratio", typeof(BasicFloat2))]
        public BasicFloat2 ViewportRatio { get; set; }
        [XmlElement("mip_levels")]
        public int MipLevels { get; set; }
        [XmlElement("mipmap_generate")]
        public bool MipMapGenerate { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public Surface()
        {
            Size = new BasicInt3 { Values = [0, 0, 0] };
            ViewportRatio = new BasicFloat2 { Values = [1f, 1f] };
            MipLevels = 0;
            MipMapGenerate = false;
        }
    }
}
