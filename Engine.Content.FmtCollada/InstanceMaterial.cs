using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class InstanceMaterial
    {
        [XmlAttribute("symbol")]
        public string Symbol { get; set; }
        [XmlAttribute("target")]
        public string Target { get; set; }
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("bind", typeof(InstanceMaterialBind))]
        public InstanceMaterialBind[] Bind { get; set; }
        [XmlElement("bind_vertex_input", typeof(InstanceMaterialBindVertexInput))]
        public InstanceMaterialBindVertexInput[] BindVertexInput { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Semantic: {SId}; Source: {Name}; Offset: {Symbol}; Set: {Target}";
        }
    }

    [Serializable]
    public class InstanceMaterialBind
    {
        [XmlAttribute("semantic")]
        public string Semantic { get; set; }
        [XmlAttribute("target")]
        public string Target { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Semantic: {Semantic}; Target: {Target};";
        }
    }

    [Serializable]
    public class InstanceMaterialBindVertexInput
    {
        [XmlAttribute("semantic")]
        public string Semantic { get; set; }
        [XmlAttribute("input_semantic")]
        public string InputSemantic { get; set; }
        [XmlAttribute("input_set")]
        public ulong InputSet { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Semantic: {Semantic}; Input: {InputSemantic}; Set: {InputSet};";
        }
    }
}
