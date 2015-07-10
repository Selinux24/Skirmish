using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    [Serializable]
    public class ProfileCG
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("platform")]
        public string Platform { get; set; }
        [XmlElement("code", typeof(Code))]
        public Code[] Code { get; set; }
        [XmlElement("include", typeof(Include))]
        public Include[] Include { get; set; }
        [XmlElement("image", typeof(Image))]
        public Image[] Images { get; set; }
        [XmlElement("newparam", typeof(NewParam))]
        public NewParam[] Params { get; set; }
        [XmlElement("technique", typeof(TechniqueFX))]
        public TechniqueFX[] Techniques { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public ProfileCG()
        {
            this.Platform = "PC";
        }

        public override string ToString()
        {
            return "ProfileCG;";
        }
    }
}
