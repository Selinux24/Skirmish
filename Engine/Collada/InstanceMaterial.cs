using System;
using System.Xml.Serialization;

namespace Engine.Collada
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

        public override string ToString()
        {
            return string.Format("SId: {0}; Name: {1}; Symbol: {2}; Target: {3};", this.SId, this.Name, this.Symbol, this.Target);
        }
    }

    [Serializable]
    public class InstanceMaterialBind
    {
        [XmlAttribute("semantic")]
        public string Semantic { get; set; }
        [XmlAttribute("target")]
        public string Target { get; set; }

        public override string ToString()
        {
            return string.Format("Semantic: {0}; Target: {1};", this.Semantic, this.Target);
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

        public override string ToString()
        {
            return string.Format("Semantic: {0}; Input: {1}; Set: {2};", this.Semantic, this.InputSemantic, this.InputSet);
        }
    }
}
