using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class MeshTechnique
    {
        [XmlArray("accessor")]
        [XmlArrayItem("param")]
        public MeshTechniqueAccessor Accessor { get; set; }
    }
}
