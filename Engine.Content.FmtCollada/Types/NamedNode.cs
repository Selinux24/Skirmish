using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class NamedNode
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(NamedNode)}; Id: {Id}; Name: {Name};";
        }
    }
}
