using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class Scene
    {
        [XmlElement("instance_physics_scene", typeof(InstanceWithExtra))]
        public InstanceWithExtra[] InstancePhysicsScene { get; set; }
        [XmlElement("instance_visual_scene", typeof(InstanceWithExtra))]
        public InstanceWithExtra InstanceVisualScene { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
