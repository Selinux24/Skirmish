using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class NewParam
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlElement("semantic")]
        public string Semantic { get; set; }
        [XmlElement("float", typeof(BasicFloat))]
        public BasicFloat Float { get; set; }
        [XmlElement("float2", typeof(BasicFloat2))]
        public BasicFloat2 Float2 { get; set; }
        [XmlElement("float3", typeof(BasicFloat3))]
        public BasicFloat3 Float3 { get; set; }
        [XmlElement("float4", typeof(BasicFloat4))]
        public BasicFloat4 Float4 { get; set; }
        [XmlElement("surface", typeof(Surface))]
        public Surface Surface { get; set; }
        [XmlElement("sampler2D", typeof(Sampler2D))]
        public Sampler2D Sampler2D { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Title: {SId}; {Semantic}";
        }
    }
}
