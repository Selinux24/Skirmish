using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    [Serializable]
    public class TechniqueCOMMON
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("image", typeof(Image))]
        public Image[] Images { get; set; }
        [XmlElement("newparam", typeof(NewParam))]
        public NewParam[] Params { get; set; }
        [XmlElement("constant", typeof(Constant))]
        public Constant Constant { get; set; }
        [XmlElement("lambert", typeof(Lambert))]
        public Lambert Lambert { get; set; }
        [XmlElement("phong", typeof(BlinnPhong))]
        public BlinnPhong Phong { get; set; }
        [XmlElement("blinn", typeof(BlinnPhong))]
        public BlinnPhong Blinn { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
        [XmlIgnore]
        public NewParam this[string param]
        {
            get
            {
                if (this.Params != null)
                {
                    return Array.Find(this.Params, p => string.Equals(p.SId, param));
                }
                else
                {
                    return null;
                }
            }
        }

        public override string ToString()
        {
            return "TechniqueCOMMON (FX); " + base.ToString();
        }
    }
}
