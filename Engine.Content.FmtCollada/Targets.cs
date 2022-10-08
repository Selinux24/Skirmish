﻿using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Targets
    {
        [XmlElement("input", typeof(Input))]
        public Input[] Inputs { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
        [XmlIgnore]
        public Input this[EnumSemantics semantic]
        {
            get
            {
                return Array.Find(this.Inputs, i => i.Semantic == semantic);
            }
        }
    }
}
