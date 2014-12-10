using System;
using System.Xml.Serialization;

namespace Engine.Collada
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

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.SId) && !string.IsNullOrEmpty(this.Name))
            {
                return string.Format("SId: {0}; Name: {1}; Semantic: {2}; Type: {3};", this.SId, this.Name, this.Semantic, this.Type);
            }
            else if (!string.IsNullOrEmpty(this.SId))
            {
                return string.Format("SId: {0}; Semantic: {1}; Type: {2};", this.SId, this.Semantic, this.Type);
            }
            else if (!string.IsNullOrEmpty(this.Name))
            {
                return string.Format("Name: {0}; Semantic: {1}; Type: {2};", this.Name, this.Semantic, this.Type);
            }
            else
            {
                return string.Format("Semantic: {0}; Type: {1};", this.Semantic, this.Type);
            }
        }
    }
}
