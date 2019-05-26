using System;
using System.Xml.Serialization;

namespace Engine
{
    [Serializable]
    public class ModularSceneryObjectActionItem
    {
        /// <summary>
        /// Item name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Object Id
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }
    }
}
