using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Scenery object state descriptor
    /// </summary>
    [Serializable]
    public class ModularSceneryObjectState
    {
        /// <summary>
        /// State name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Transitions list
        /// </summary>
        [XmlArray("transitions")]
        [XmlArrayItem("transition", typeof(ModularSceneryObjectStateTransition))]
        public ModularSceneryObjectStateTransition[] Transitions { get; set; } = null;
    }
}
