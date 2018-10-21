using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class VarFloatOrParam
    {
        [XmlElement("float", typeof(BasicFloat))]
        public BasicFloat Float { get; set; }
        [XmlElement("param", typeof(BasicParam))]
        public BasicParam Param { get; set; }

        public override string ToString()
        {
            if (this.Float != null) return this.Float.ToString();
            else if (this.Param != null) return this.Param.ToString();
            else return "Empty";
        }
    }
}
