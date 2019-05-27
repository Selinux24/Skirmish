using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Scenery object state transition descriptor
    /// </summary>
    [Serializable]
    public class ModularSceneryObjectStateTransition
    {
        /// <summary>
        /// State name
        /// </summary>
        [XmlAttribute("state")]
        public string State { get; set; }
    }
}
