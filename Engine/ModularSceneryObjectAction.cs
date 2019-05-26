using System;
using System.Xml.Serialization;

namespace Engine
{
    [Serializable]
    public class ModularSceneryObjectAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Triggered item list
        /// </summary>
        [XmlArray("items")]
        [XmlArrayItem("item", typeof(ModularSceneryObjectActionItem))]
        public ModularSceneryObjectActionItem[] Items { get; set; } = null;
    }
}
