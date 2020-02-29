using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Annotate
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("bool")]
        public BasicBool Bool { get; set; }
        [XmlElement("bool2")]
        public BasicBool2 Bool2 { get; set; }
        [XmlElement("bool3")]
        public BasicBool3 Bool3 { get; set; }
        [XmlElement("bool4")]
        public BasicBool4 Bool4 { get; set; }
        [XmlElement("int")]
        public BasicInt Int { get; set; }
        [XmlElement("int2")]
        public BasicInt2 Int2 { get; set; }
        [XmlElement("int3")]
        public BasicInt3 Int3 { get; set; }
        [XmlElement("int4")]
        public BasicInt4 Int4 { get; set; }
        [XmlElement("float")]
        public BasicFloat Float { get; set; }
        [XmlElement("float2")]
        public BasicFloat2 Float2 { get; set; }
        [XmlElement("float3")]
        public BasicFloat3 Float3 { get; set; }
        [XmlElement("float4")]
        public BasicFloat4 Float4 { get; set; }
        [XmlElement("float2x2")]
        public BasicFloat2X2 Float2x2 { get; set; }
        [XmlElement("float3x3")]
        public BasicFloat3X3 Float3x3 { get; set; }
        [XmlElement("float4x4")]
        public BasicFloat4X4 Float4x4 { get; set; }
        [XmlElement("string")]
        public string String { get; set; }
    }
}
