using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class NamedNode
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Id) && !string.IsNullOrEmpty(this.Name))
                return string.Format("Id: {0}; Name: {1};", this.Id, this.Name);
            if (!string.IsNullOrEmpty(this.Id)) 
                return string.Format("Id: {0};", this.Id);
            if (!string.IsNullOrEmpty(this.Name)) 
                return string.Format("Name: {0};", this.Name);

            return "";
        }
    }
}
