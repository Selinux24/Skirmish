using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class MatrixType : FloatArrayType
    {
        [XmlAttribute("sid")]
        public string Id { get; set; }
    }
}
