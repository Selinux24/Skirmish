using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SharpDX;

namespace Common.Collada
{
    using Common.Collada.Types;
    using Common.Utils;

    [Serializable]
    public class LibraryGeometry
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("mesh")]
        public Mesh Mesh { get; set; }

        private Vector3[] GetVector3Array(int index, Matrix transform)
        {
            List<Vector3> res = new List<Vector3>();

            float[] vList = this.Mesh.Sources[index].FloatArray.Value.ToArray();

            for (int i = 0; i < vList.Length; i += 3)
            {
                Vector3 v = new Vector3(
                    vList[i + 0],
                    vList[i + 1],
                    vList[i + 2]);

                Vector4 v4 = Vector3.Transform(v, transform);

                res.Add(new Vector3(v4.X, v4.Y, v4.Z));
            }

            return res.ToArray();
        }

        private Vector2[] GetVector2Array(int index, Matrix transform)
        {
            List<Vector2> res = new List<Vector2>();

            float[] vList = this.Mesh.Sources[index].FloatArray.Value.ToArray();

            for (int i = 0; i < vList.Length; i += 2)
            {
                Vector2 v = new Vector2(
                    vList[i + 0],
                    vList[i + 1]);

                res.Add(v);
            }

            return res.ToArray();
        }

        private Color4[] GetColor4Array(int index)
        {
            List<Color4> res = new List<Color4>();

            float[] vList = this.Mesh.Sources[index].FloatArray.Value.ToArray();

            for (int i = 0; i < vList.Length; i += 4)
            {
                Color4 c = new Color4(
                    vList[i + 0],
                    vList[i + 1],
                    vList[i + 2],
                    vList[i + 3]);

                res.Add(c);
            }

            return res.ToArray();
        }

        private IVertex[] MapPositionColor(Matrix translation, Matrix rotation, Matrix scale)
        {
            int sourceCount = this.Mesh.Sources.Count;
            int vertexOffset = this.Mesh.PolyList[InputSemantics.VERTEX];
            int colorOffset = this.Mesh.PolyList[InputSemantics.COLOR];

            List<IVertex> verts = new List<IVertex>();

            Vector3[] vertices = vertexOffset >= 0 ? this.GetVector3Array(vertexOffset, rotation * scale * translation) : null;
            Color4[] colors = colorOffset >= 0 ? this.GetColor4Array(colorOffset) : null;

            int[] pList = this.Mesh.PolyList.P.ToArray();

            for (int i = 0; i < pList.Length; i += sourceCount)
            {
                VertexPositionColor v = new VertexPositionColor()
                {
                    Position = vertexOffset >= 0 ? vertices[pList[i + vertexOffset]] : Vector3.Zero,
                    Color = colorOffset >= 0 ? colors[pList[i + colorOffset]] : Color4.Black,
                };

                verts.Add(v);
            }

            return verts.ToArray();
        }

        private IVertex[] MapPositionNormalColor(Matrix translation, Matrix rotation, Matrix scale, bool normalizeNormals)
        {
            int sourceCount = this.Mesh.Sources.Count;
            int vertexOffset = this.Mesh.PolyList[InputSemantics.VERTEX];
            int normalOffset = this.Mesh.PolyList[InputSemantics.NORMAL];
            int colorOffset = this.Mesh.PolyList[InputSemantics.COLOR];

            List<IVertex> verts = new List<IVertex>();

            Vector3[] vertices = vertexOffset >= 0 ? this.GetVector3Array(vertexOffset, rotation * scale * translation) : null;
            Vector3[] normals = normalOffset >= 0 ? this.GetVector3Array(normalOffset, rotation * scale) : null;
            Color4[] colors = colorOffset >= 0 ? this.GetColor4Array(colorOffset) : null;

            int[] pList = this.Mesh.PolyList.P.ToArray();

            for (int i = 0; i < pList.Length; i += sourceCount)
            {
                VertexPositionNormalColor v = new VertexPositionNormalColor()
                {
                    Position = vertexOffset >= 0 ? vertices[pList[i + vertexOffset]] : Vector3.Zero,
                    Normal = normalOffset >= 0 ? normals[pList[i + normalOffset]] : Vector3.Zero,
                    Color = colorOffset >= 0 ? colors[pList[i + colorOffset]] : Color4.Black,
                };

                if (normalizeNormals) v.Normal.Normalize();

                verts.Add(v);
            }

            return verts.ToArray();
        }

        private IVertex[] MapPositionTexture(Matrix translation, Matrix rotation, Matrix scale)
        {
            int sourceCount = this.Mesh.Sources.Count;
            int vertexOffset = this.Mesh.PolyList[InputSemantics.VERTEX];
            int textureOffset = this.Mesh.PolyList[InputSemantics.TEXCOORD];

            List<IVertex> verts = new List<IVertex>();

            Vector3[] vertices = vertexOffset >= 0 ? this.GetVector3Array(vertexOffset, rotation * scale * translation) : null;
            Vector2[] textures = textureOffset >= 0 ? this.GetVector2Array(textureOffset, Matrix.Identity) : null;

            int[] pList = this.Mesh.PolyList.P.ToArray();

            for (int i = 0; i < pList.Length; i += sourceCount)
            {
                VertexPositionTexture v = new VertexPositionTexture()
                {
                    Position = vertexOffset >= 0 ? vertices[pList[i + vertexOffset]] : Vector3.Zero,
                    Texture = textureOffset >= 0 ? textures[pList[i + textureOffset]] : Vector2.Zero,
                };

                verts.Add(v);
            }

            return verts.ToArray();
        }

        private IVertex[] MapPositionNormalTexture(Matrix translation, Matrix rotation, Matrix scale, bool normalizeNormals)
        {
            int sourceCount = this.Mesh.Sources.Count;
            int vertexOffset = this.Mesh.PolyList[InputSemantics.VERTEX];
            int normalOffset = this.Mesh.PolyList[InputSemantics.NORMAL];
            int textureOffset = this.Mesh.PolyList[InputSemantics.TEXCOORD];

            List<IVertex> verts = new List<IVertex>();

            Vector3[] vertices = vertexOffset >= 0 ? this.GetVector3Array(vertexOffset, rotation * scale * translation) : null;
            Vector3[] normals = normalOffset >= 0 ? this.GetVector3Array(normalOffset, rotation * scale) : null;
            Vector2[] textures = textureOffset >= 0 ? this.GetVector2Array(textureOffset, Matrix.Identity) : null;

            int[] pList = this.Mesh.PolyList.P.ToArray();

            for (int i = 0; i < pList.Length; i += sourceCount)
            {
                VertexPositionNormalTexture v = new VertexPositionNormalTexture()
                {
                    Position = vertexOffset >= 0 ? vertices[pList[i + vertexOffset]] : Vector3.Zero,
                    Normal = normalOffset >= 0 ? normals[pList[i + normalOffset]] : Vector3.Zero,
                    Texture = textureOffset >= 0 ? textures[pList[i + textureOffset]] : Vector2.Zero,
                };

                if (normalizeNormals) v.Normal.Normalize();

                verts.Add(v);
            }

            return verts.ToArray();
        }

        private IVertex[] MapPositionNormal(Matrix translation, Matrix rotation, Matrix scale, bool normalizeNormals)
        {
            int sourceCount = this.Mesh.Sources.Count;
            int vertexOffset = this.Mesh.PolyList[InputSemantics.VERTEX];
            int normalOffset = this.Mesh.PolyList[InputSemantics.NORMAL];

            List<IVertex> verts = new List<IVertex>();

            Vector3[] vertices = vertexOffset >= 0 ? this.GetVector3Array(vertexOffset, rotation * scale * translation) : null;
            Vector3[] normals = normalOffset >= 0 ? this.GetVector3Array(normalOffset, rotation * scale) : null;

            int[] pList = this.Mesh.PolyList.P.ToArray();

            for (int i = 0; i < pList.Length; i += sourceCount)
            {
                VertexPositionNormalColor v = new VertexPositionNormalColor()
                {
                    Position = vertexOffset >= 0 ? vertices[pList[i + vertexOffset]] : Vector3.Zero,
                    Normal = normalOffset >= 0 ? normals[pList[i + normalOffset]] : Vector3.Zero,
                    Color = Color.White,
                };

                if (normalizeNormals) v.Normal.Normalize();

                verts.Add(v);
            }

            return verts.ToArray();
        }

        private IVertex[] MapPosition(Matrix translation, Matrix rotation, Matrix scale)
        {
            int sourceCount = this.Mesh.Sources.Count;
            int vertexOffset = this.Mesh.PolyList[InputSemantics.VERTEX];

            List<IVertex> verts = new List<IVertex>();

            Vector3[] vertices = vertexOffset >= 0 ? this.GetVector3Array(vertexOffset, rotation * scale * translation) : null;

            int[] pList = this.Mesh.PolyList.P.ToArray();

            for (int i = 0; i < pList.Length; i += sourceCount)
            {
                VertexPositionColor v = new VertexPositionColor()
                {
                    Position = vertexOffset >= 0 ? vertices[pList[i + vertexOffset]] : Vector3.Zero,
                    Color = Color.White,
                };

                verts.Add(v);
            }

            return verts.ToArray();
        }

        public IVertex[] Map(Matrix translation, Matrix rotation, Matrix scale, bool normalizeNormals)
        {
            List<IVertex> vertices = new List<IVertex>();

            if (this.Mesh.PolyList.Input.Exists(i => i.Semantic == InputSemantics.VERTEX))
            {
                if (this.Mesh.PolyList.Input.Exists(i => i.Semantic == InputSemantics.NORMAL))
                {
                    if (this.Mesh.PolyList.Input.Exists(i => i.Semantic == InputSemantics.COLOR))
                    {
                        vertices.AddRange(this.MapPositionNormalColor(translation, rotation, scale, normalizeNormals));
                    }
                    else if (this.Mesh.PolyList.Input.Exists(i => i.Semantic == InputSemantics.TEXCOORD))
                    {
                        vertices.AddRange(this.MapPositionNormalTexture(translation, rotation, scale, normalizeNormals));
                    }
                    else
                    {
                        vertices.AddRange(this.MapPositionNormal(translation, rotation, scale, normalizeNormals));
                    }
                }
                else
                {
                    if (this.Mesh.PolyList.Input.Exists(i => i.Semantic == InputSemantics.COLOR))
                    {
                        vertices.AddRange(this.MapPositionColor(translation, rotation, scale));
                    }
                    else if (this.Mesh.PolyList.Input.Exists(i => i.Semantic == InputSemantics.TEXCOORD))
                    {
                        vertices.AddRange(this.MapPositionTexture(translation, rotation, scale));
                    }
                    else
                    {
                        vertices.AddRange(this.MapPosition(translation, rotation, scale));
                    }
                }
            }

            return vertices.ToArray();
        }
    }
}
