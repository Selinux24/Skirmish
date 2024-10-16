﻿using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Source : NamedNode
    {
        [XmlElement("asset")]
        public Asset Asset { get; set; }

        [XmlElement("IDREF_array", typeof(NamedIdRefArray))]
        public NamedIdRefArray IdRefArray { get; set; }
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

        /// <inheritdoc/>
        public override string ToString()
        {
            string typeName = "None";
            if (IdRefArray != null) typeName = nameof(IdRefArray);
            if (NameArray != null) typeName = nameof(NameArray);
            if (BoolArray != null) typeName = nameof(BoolArray);
            if (IntArray != null) typeName = nameof(IntArray);
            if (FloatArray != null) typeName = nameof(FloatArray);

            return base.ToString() + $" Type: {typeName}";
        }
    }
}
