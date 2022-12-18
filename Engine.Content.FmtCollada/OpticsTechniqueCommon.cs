﻿using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class OpticsTechniqueCommon
    {
        [XmlElement("orthographic", typeof(Orthographic))]
        public Orthographic Orthographic { get; set; }
        [XmlElement("perspective", typeof(Perspective))]
        public Perspective Perspective { get; set; }
    }
}
