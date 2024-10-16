using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

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
            Start = 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString() + $" Start: {Start}; End {End};";
        }
    }
}
