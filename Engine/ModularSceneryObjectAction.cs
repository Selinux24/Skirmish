using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Modular scenery object action
    /// </summary>
    [Serializable]
    public class ModularSceneryObjectAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// State from name
        /// </summary>
        [XmlAttribute("state_from")]
        public string StateFrom { get; set; }
        /// <summary>
        /// State to name
        /// </summary>
        [XmlAttribute("state_to")]
        public string StateTo { get; set; }
        /// <summary>
        /// Animation plan name
        /// </summary>
        [XmlAttribute("animation_plan")]
        public string AnimationPlan { get; set; }
        /// <summary>
        /// Triggered item list
        /// </summary>
        [XmlArray("items")]
        [XmlArrayItem("item", typeof(ModularSceneryObjectActionItem))]
        public ModularSceneryObjectActionItem[] Items { get; set; } = null;
    }
}
