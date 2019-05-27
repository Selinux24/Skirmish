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
        /// Object Id
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }
        /// <summary>
        /// Action name
        /// </summary>
        [XmlAttribute("action")]
        public string Action { get; set; }
    }
}
