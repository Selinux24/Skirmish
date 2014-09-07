using System.Collections.Generic;
using SharpDX;

namespace Common.Utils
{
    public struct Vertex
    {
        public Vector3? Position;
        public Vector3? Normal;
        public Vector3? Tangent;
        public Vector2? Texture;
        public Color4? Color;
        public Vector2? Size;

        public static Vertex CreateVertexPosition(Vector3 position)
        {
            return new Vertex()
            {
                Position = position,
            };
        }
        public static Vertex CreateVertexPositionColor(Vector3 position, Color4 color)
        {
            return new Vertex()
            {
                Position = position,
                Color = color,
            };
        }
        public static Vertex CreateVertexPositionTexture(Vector3 position, Vector2 texture)
        {
            return new Vertex()
            {
                Position = position,
                Texture = texture,
            };
        }
        public static Vertex CreateVertexPositionNormalColor(Vector3 position, Vector3 normal, Color4 color)
        {
            return new Vertex()
            {
                Position = position,
                Normal = normal,
                Color = color,
            };
        }
        public static Vertex CreateVertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texture)
        {
            return new Vertex()
            {
                Position = position,
                Normal = normal,
                Texture = texture,
            };
        }
        public static Vertex CreateVertexPositionNormalTangentTexture(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texture)
        {
            return new Vertex()
            {
                Position = position,
                Normal = normal,
                Tangent = tangent,
                Texture = texture,
            };
        }
        public static Vertex CreateVertexBillboard(Vector3 position, Vector2 size)
        {
            return new Vertex()
            {
                Position = position,
                Size = size,
            };
        }

        public static IList<T> Convert<T>(Vertex[] verts) where T : struct, IVertex
        {
            List<T> list = new List<T>();

            foreach (Vertex vert in verts)
            {
                list.Add(Vertex.Convert<T>(vert));
            }

            return list;
        }
        public static T Convert<T>(Vertex vert) where T : struct, IVertex
        {
            T o = default(T);

            return (T)o.Convert(vert);
        }

        public static Vector3[] GetPositions(Vertex[] vertices)
        {
            List<Vector3> res = new List<Vector3>();

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Position.HasValue)
                {
                    res.Add(vertices[i].Position.Value);
                }
            }

            return res.ToArray();
        }
        public static Vector3[] GetNormals(Vertex[] vertices)
        {
            List<Vector3> res = new List<Vector3>();

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Normal.HasValue)
                {
                    res.Add(vertices[i].Normal.Value);
                }
            }

            return res.ToArray();
        }
        public static Vector3[] GetTangents(Vertex[] vertices)
        {
            List<Vector3> res = new List<Vector3>();

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Tangent.HasValue)
                {
                    res.Add(vertices[i].Tangent.Value);
                }
            }

            return res.ToArray();
        }
        public static Vector2[] GetTextures(Vertex[] vertices)
        {
            List<Vector2> res = new List<Vector2>();

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Texture.HasValue)
                {
                    res.Add(vertices[i].Texture.Value);
                }
            }

            return res.ToArray();
        }
        public static Color4[] GetColors(Vertex[] vertices)
        {
            List<Color4> res = new List<Color4>();

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Color.HasValue)
                {
                    res.Add(vertices[i].Color.Value);
                }
            }

            return res.ToArray();
        }
        public static Vector2[] GetSizes(Vertex[] vertices)
        {
            List<Vector2> res = new List<Vector2>();

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Size.HasValue)
                {
                    res.Add(vertices[i].Size.Value);
                }
            }

            return res.ToArray();
        }
    }
}
