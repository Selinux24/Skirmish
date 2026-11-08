using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class Render
    {
        [XmlAttribute("camera_node")]
        public string CameraNode { get; set; }
        [XmlElement("layer", typeof(string))]
        public string[] Layer { get; set; }
        [XmlElement("instance_effect", typeof(InstanceEffect))]
        public InstanceEffect InstanceEffect { get; set; }
    }
}
