using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    [Serializable]
    public class TechniqueFX
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlElement("annotate", typeof(Annotate))]
        public Annotate[] Annotates { get; set; }
        [XmlElement("code", typeof(Code))]
        public Code[] Code { get; set; }
        [XmlElement("include", typeof(Include))]
        public Include[] Include { get; set; }
        [XmlElement("image", typeof(Image))]
        public Image[] Images { get; set; }
        [XmlElement("newparam", typeof(NewParam))]
        public NewParam[] NewParams { get; set; }
        [XmlElement("setparam", typeof(SetParam))]
        public SetParam[] SetParams { get; set; }
        [XmlElement("pass", typeof(Pass))]
        public Pass[] Passes { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public override string ToString()
        {
            return "Technique (FX); " + base.ToString();
        }
    }
}
