using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class Contributor
    {
        [XmlElement("author")]
        public string Author { get; set; }
        [XmlElement("author_email")]
        public string AuthorEmail { get; set; }
        [XmlElement("author_website")]
        public string AuthorWebsite { get; set; }
        [XmlElement("authoring_tool")]
        public string AuthoringTool { get; set; }
        [XmlElement("comments")]
        public string Comments { get; set; }
        [XmlElement("copyright")]
        public string Copyright { get; set; }
        [XmlElement("source_data")]
        public string SourceData { get; set; }

        public override string ToString()
        {
            return string.Format("Author: {0}; Tool: {1}; Copyright: {2}", this.Author, this.AuthoringTool, this.Copyright);
        }
    }
}
