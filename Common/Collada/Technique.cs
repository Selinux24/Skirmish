using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class Technique
    {
        [XmlAttribute("sid")]
        public string Id { get; set; }
        [XmlElement("constant")]
        public TechniqueDescription Constant { get; set; }
        [XmlElement("lambert")]
        public TechniqueDescription Lambert { get; set; }
        [XmlElement("phong")]
        public TechniqueDescription Phong { get; set; }
        [XmlElement("blinn")]
        public TechniqueDescription Blinn { get; set; }
        [XmlIgnore()]
        public TechniqueDescription Description
        {
            get
            {
                if (this.Constant != null) return this.Constant;
                if (this.Lambert != null) return this.Lambert;
                if (this.Phong != null) return this.Phong;
                if (this.Blinn != null) return this.Blinn;
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}; {1}", this.Id, this.Description);
        }
    }
}
