using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Source : NamedNode
    {
        [XmlElement("asset")]
        public Asset Asset { get; set; }

        [XmlElement("IDREF_array", typeof(NamedIDREFArray))]
        public NamedIDREFArray IDREFArray { get; set; }
        [XmlElement("Name_array", typeof(NamedNameArray))]
        public NamedNameArray NameArray { get; set; }
        [XmlElement("bool_array", typeof(NamedBoolArray))]
        public NamedBoolArray BoolArray { get; set; }
        [XmlElement("int_array", typeof(NamedIntArray))]
        public NamedIntArray IntArray { get; set; }
        [XmlElement("float_array", typeof(NamedFloatArray))]
        public NamedFloatArray FloatArray { get; set; }

        [XmlElement("technique_common", typeof(SourceTechniqueCommon))]
        public SourceTechniqueCommon TechniqueCommon { get; set; }

        [XmlElement("technique", typeof(Technique))]
        public Technique Technique { get; set; }

        public override string ToString()
        {
            return "Source; " + base.ToString();
        }
    }
}
