using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Modular scenery animation plan
    /// </summary>
    [Serializable]
    public class ModularSceneryObjectAnimationPlan
    {
        /// <summary>
        /// Plan name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Default animation
        /// </summary>
        [XmlAttribute("default")]
        public bool Default { get; set; } = false;
        /// <summary>
        /// Plan's animation paths
        /// </summary>
        [XmlArray("paths")]
        [XmlArrayItem("path", typeof(ModularSceneryObjectAnimationPath))]
        public ModularSceneryObjectAnimationPath[] Paths { get; set; } = null;
    }
}
