using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Common.Collada
{
    using Common.Collada.Types;

    [Serializable]
    public class LibraryEffect
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlElement("profile_COMMON")]
        public Profile Profile { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.Id);
        }
    }
}
