using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public enum EnumSemantics
    {
        [XmlEnum("BINORMAL")]
        Binormal,
        [XmlEnum("CONTINUITY")]
        Continuity,
        [XmlEnum("IMAGE")]
        Image,
        [XmlEnum("INPUT")]
        Input,
        [XmlEnum("WEIGHT")]
        Weight,
        [XmlEnum("INTERPOLATION")]
        Interpolation,
        [XmlEnum("INV_BIND_MATRIX")]
        InverseBindMatrix,
        [XmlEnum("UV")]
        UV,
        [XmlEnum("VERTEX")]
        Vertex,
        [XmlEnum("JOINT")]
        Joint,
        [XmlEnum("LINEAR_STEPS")]
        LinearSteps,
        [XmlEnum("NORMAL")]
        Normal,
        [XmlEnum("OUTPUT")]
        Output,
        [XmlEnum("COLOR")]
        Color,
        [XmlEnum("TEXCOORD")]
        TexCoord,
        [XmlEnum("POSITION")]
        Position,
        [XmlEnum("MORPH_TARGET")]
        MorphTarget,
        [XmlEnum("MORPH_WEIGHT")]
        MorphWeight,
        [XmlEnum("TANGENT")]
        Tangent,
        [XmlEnum("IN_TANGENT")]
        InTangent,
        [XmlEnum("OUT_TANGENT")]
        OutTangent,
    }
}
