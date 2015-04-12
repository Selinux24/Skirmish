using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SharpDX;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Source : NamedNode
    {
        [XmlElement("asset")]
        public Asset Asset { get; set; }

        [XmlElement("IDREF_array", typeof(NamedIDREFArray))]
        public NamedIDREFArray IDREFArray { get; set; }
        [XmlElement("Name_array", typeof(NamedNameArray))]
        public NamedNameArray NameArray { get; set; }
        [XmlElement("bool_array", typeof(NamedBoolArray))]
        public NamedBoolArray BoolArray { get; set; }
        [XmlElement("int_array", typeof(NamedIntArray))]
        public NamedIntArray IntArray { get; set; }
        [XmlElement("float_array", typeof(NamedFloatArray))]
        public NamedFloatArray FloatArray { get; set; }

        [XmlElement("technique_common", typeof(SourceTechniqueCommon))]
        public SourceTechniqueCommon TechniqueCommon { get; set; }

        [XmlElement("technique", typeof(Technique))]
        public Technique Technique { get; set; }

        public float[] ReadFloat()
        {
            int stride = this.TechniqueCommon.Accessor.Stride;
            if (stride != 1)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(float)));
            }

            int length = this.TechniqueCommon.Accessor.Count;

            List<float> n = new List<float>();

            for (int i = 0; i < length * stride; i += stride)
            {
                float v = this.FloatArray[i];

                n.Add(v);
            }

            return n.ToArray();
        }
        public string[] ReadString()
        {
            int stride = this.TechniqueCommon.Accessor.Stride;
            if (stride != 1)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(string)));
            }

            int length = this.TechniqueCommon.Accessor.Count;

            List<string> names = new List<string>();

            for (int i = 0; i < length * stride; i += stride)
            {
                string v = this.NameArray[i];

                names.Add(v);
            }

            return names.ToArray();
        }
        public Vector2[] ReadVector2()
        {
            int stride = this.TechniqueCommon.Accessor.Stride;
            if (stride != 2)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(Vector2)));
            }

            int length = this.TechniqueCommon.Accessor.Count;

            int s = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "S");
            int t = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "T");

            List<Vector2> verts = new List<Vector2>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Vector2 v = new Vector2(
                    this.FloatArray[i + s],
                    this.FloatArray[i + t]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        public Vector3[] ReadVector3()
        {
            int stride = this.TechniqueCommon.Accessor.Stride;
            if (stride != 3)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(Vector3)));
            }

            int x = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "X");
            int y = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "Y");
            int z = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "Z");

            int length = this.TechniqueCommon.Accessor.Count;

            List<Vector3> verts = new List<Vector3>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Vector3 v = new Vector3(
                    this.FloatArray[i + x],
                    this.FloatArray[i + y],
                    this.FloatArray[i + z]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        public Vector4[] ReadVector4()
        {
            int stride = this.TechniqueCommon.Accessor.Stride;
            if (stride != 4)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(Vector3)));
            }

            int x = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "X");
            int y = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "Y");
            int z = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "Z");
            int w = Array.FindIndex(this.TechniqueCommon.Accessor.Params, p => p.Name == "W");

            int length = this.TechniqueCommon.Accessor.Count;

            List<Vector4> verts = new List<Vector4>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Vector4 v = new Vector4(
                    this.FloatArray[i + x],
                    this.FloatArray[i + y],
                    this.FloatArray[i + z],
                    this.FloatArray[i + w]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        public Matrix[] ReadMatrix()
        {
            int stride = this.TechniqueCommon.Accessor.Stride;
            if (stride != 16)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(Matrix)));
            }

            int length = this.TechniqueCommon.Accessor.Count;

            List<Matrix> mats = new List<Matrix>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Matrix m = new Matrix()
                {
                    M11 = this.FloatArray[i + 0],
                    M12 = this.FloatArray[i + 1],
                    M13 = this.FloatArray[i + 2],
                    M14 = this.FloatArray[i + 3],

                    M21 = this.FloatArray[i + 4],
                    M22 = this.FloatArray[i + 5],
                    M23 = this.FloatArray[i + 6],
                    M24 = this.FloatArray[i + 7],

                    M31 = this.FloatArray[i + 8],
                    M32 = this.FloatArray[i + 9],
                    M33 = this.FloatArray[i + 10],
                    M34 = this.FloatArray[i + 11],

                    M41 = this.FloatArray[i + 12],
                    M42 = this.FloatArray[i + 13],
                    M43 = this.FloatArray[i + 14],
                    M44 = this.FloatArray[i + 15],
                };

                mats.Add(m);
            }

            return mats.ToArray();
        }

        public override string ToString()
        {
            return "Source; " + base.ToString();
        }
    }
}
