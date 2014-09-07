using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class Scene
    {
        [XmlElement("instance_visual_scene")]
        public InstanceVisualScene InstanceVisualScene { get; set; }
    }
}
