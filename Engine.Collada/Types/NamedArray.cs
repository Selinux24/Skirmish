using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    public class NamedArray : NamedNode
    {
        [XmlAttribute("count")]
        public int Count { get; set; }

        public override string ToString()
        {
            return string.Format("Count: {0}; ", this.Count) + base.ToString();
        }
    }
}
