using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class VisualScene : NamedNode
    {
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("node", typeof(Node))]
        public Node[] Nodes { get; set; }
        [XmlElement("evaluate_scene", typeof(EvaluateScene))]
        public EvaluateScene[] EvaluateScenes { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
