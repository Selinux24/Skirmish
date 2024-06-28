using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlText]
        public string Text
        {
            get
            {
                return Collada.ConvertToString(Value);
            }
            set
            {
                Value = Collada.Convert<float>(value);
            }
        }
        [XmlIgnore]
        public float Value { get; set; }

        public BasicFloat()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Value}";
        }
    }
}
