using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    [Serializable]
    public class ProfileCOMMON
    {
        [XmlElement("image", typeof(Image))]
        public Image[] Images { get; set; }
        [XmlElement("newparam", typeof(NewParam))]
        public NewParam[] Params { get; set; }
        [XmlElement("technique", typeof(TechniqueCOMMON))]
        public TechniqueCOMMON Technique { get; set; }
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
            return "ProfileCOMMON;";
        }
    }
}
