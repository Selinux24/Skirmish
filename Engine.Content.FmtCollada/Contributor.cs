using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class Contributor
    {
        [XmlElement("author")]
        public string Author { get; set; }
        [XmlElement("authoring_tool")]
        public string AuthoringTool { get; set; }
        [XmlElement("comments")]
        public string Comments { get; set; }
        [XmlElement("copyright")]
        public string Copyright { get; set; }
        [XmlElement("source_data")]
        public string SourceData { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Author: {Author}; Tool: {AuthoringTool};";
        }
    }
}
