﻿using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Extra : NamedNode
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("technique", typeof(Technique))]
        public Technique[] Techniques { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString() + $" Type: {Type};";
        }
    }
}
