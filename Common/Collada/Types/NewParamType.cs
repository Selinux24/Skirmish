using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class NewParamType
    {
        [XmlAttribute("sid")]
        public string Id { get; set; }
        [XmlElement("semantic")]
        public string Semantic { get; set; }
        [XmlElement("float", typeof(FloatType))]
        public FloatType Float { get; set; }
        [XmlElement("float2", typeof(Float2Type))]
        public Float2Type Float2 { get; set; }
        [XmlElement("float3", typeof(Float3Type))]
        public Float3Type Float3 { get; set; }
        [XmlElement("float4", typeof(Float4Type))]
        public Float4Type Float4 { get; set; }
        [XmlElement("surface", typeof(SurfaceType))]
        public SurfaceType Surface { get; set; }
        [XmlElement("sampler2D", typeof(Sampler2DType))]
        public Sampler2DType Sampler2D { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.Id);
        }
    }
}
