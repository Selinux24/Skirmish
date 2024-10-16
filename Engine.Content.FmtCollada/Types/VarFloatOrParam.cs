using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class VarFloatOrParam
    {
        [XmlElement("float", typeof(BasicFloat))]
        public BasicFloat Float { get; set; }
        [XmlElement("param", typeof(BasicParam))]
        public BasicParam Param { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return
                Float?.ToString() ??
                Param?.ToString() ??
                "Empty";
        }
    }
}
