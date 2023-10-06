using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    public class NamedArray : NamedNode
    {
        [XmlAttribute("count")]
        public int Count { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString() + $" Count: {Count};";
        }
    }
}
