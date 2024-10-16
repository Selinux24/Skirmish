﻿using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class Ph
    {
        [XmlElement("p", typeof(BasicIntArray))]
        public BasicIntArray P { get; set; }
        [XmlElement("h", typeof(BasicIntArray))]
        public BasicIntArray[] H { get; set; }
    }
}
