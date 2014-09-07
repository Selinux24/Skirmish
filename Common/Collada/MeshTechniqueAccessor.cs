using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Collada
{
    using Common.Collada.Types;

    [Serializable]
    public class MeshTechniqueAccessor : List<ParamType>
    {
        [XmlAttribute("source")]
        public string Source { get; set; }
        [XmlAttribute("count")]
        public int ItemCount { get; set; }
        [XmlAttribute("stride")]
        public int Stride { get; set; }
    }
}
