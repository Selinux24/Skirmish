using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Object action item
    /// </summary>
    /// <remarks>Designates the action by name to activate in the referenced object by id</remarks>
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
