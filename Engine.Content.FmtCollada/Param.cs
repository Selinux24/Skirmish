using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class Param
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("semantic")]
        public string Semantic { get; set; }
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(SId) && !string.IsNullOrEmpty(Name))
            {
                return $"SId: {SId}; Name: {Name}; Semantic: {Semantic}; Type: {Type};";
            }
            else if (!string.IsNullOrEmpty(SId))
            {
                return $"SId: {SId}; Semantic: {Semantic}; Type: {Type};";
            }
            else if (!string.IsNullOrEmpty(Name))
            {
                return $"Name: {Name}; Semantic: {Semantic}; Type: {Type};";
            }
            else
            {
                return $"Semantic: {Semantic}; Type: {Type};";
            }
        }
    }
}
