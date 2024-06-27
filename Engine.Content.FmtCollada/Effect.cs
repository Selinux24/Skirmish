using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;
    using Engine.Collada.FX;

    [Serializable]
    public class Effect : NamedNode
    {
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("annotate", typeof(Annotate))]
        public Annotate[] Annotations { get; set; }
        [XmlElement("image", typeof(Image))]
        public Image[] Images { get; set; }
        [XmlElement("newparam", typeof(NewParam))]
        public NewParam[] Params { get; set; }
        [XmlElement("profile_COMMON", typeof(ProfileCommon))]
        public ProfileCommon ProfileCommon { get; set; }
        [XmlElement("profile_CG", typeof(ProfileCG))]
        public ProfileCG ProfileCG { get; set; }
        [XmlElement("profile_GLES", typeof(ProfileGles))]
        public ProfileGles ProfileGles { get; set; }
        [XmlElement("profile_GLSL", typeof(ProfileGlsl))]
        public ProfileGlsl ProfileGlsl { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
