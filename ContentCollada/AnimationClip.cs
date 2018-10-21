using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class AnimationClip : NamedNode
    {
        [XmlAttribute("start")]
        public double Start { get; set; }
        [XmlAttribute("end")]
        public double End { get; set; }
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("instance_animation", typeof(InstanceWithExtra))]
        public InstanceWithExtra[] InstanceAnimations { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public AnimationClip()
        {
            this.Start = 0;
        }

        public override string ToString()
        {
            return string.Format("AnimationClip; Start: {0}; End {1}; ", this.Start, this.End) + base.ToString();
        }
    }
}
