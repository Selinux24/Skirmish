using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    [Serializable]
    public class ProfileCommon
    {
        [XmlElement("image", typeof(Image))]
        public Image[] Images { get; set; }
        [XmlElement("newparam", typeof(NewParam))]
        public NewParam[] Params { get; set; }
        [XmlElement("technique", typeof(TechniqueCommon))]
        public TechniqueCommon Technique { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
        [XmlIgnore]
        public NewParam this[string param]
        {
            get
            {
                if (Params != null)
                {
                    return Array.Find(Params, p => string.Equals(p.SId, param));
                }
                else
                {
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ProfileCommon)};";
        }
    }
}
