﻿using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Triangles : MeshElements
    {
        [XmlElement("p", typeof(BasicIntArray))]
        public BasicIntArray P { get; set; }
    }
}
