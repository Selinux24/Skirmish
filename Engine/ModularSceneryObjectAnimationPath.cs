using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Modular scenery animation path
    /// </summary>
    [Serializable]
    public class ModularSceneryObjectAnimationPath
    {
        /// <summary>
        /// Path name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
