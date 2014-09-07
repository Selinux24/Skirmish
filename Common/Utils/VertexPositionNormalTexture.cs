using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using System.Collections.Generic;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTexture : IVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionNormalTexture));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexPositionNormalTexture[] Create(Vector3[] positions, Vector3[] normals, Vector2[] uvs)
        {
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();

            for (int i = 0; i < positions.Length; i++)
            {
                vertices.Add(new VertexPositionNormalTexture()
                {
                    Position = positions[i],
                    Normal = normals[i],
                    Texture = uvs[i],
                });
            }

            return vertices.ToArray();
        }
        public static VertexPositionNormalTexture[] Create(Vertex[] vertexes)
        {
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();

            for (int i = 0; i < vertexes.Length; i++)
            {
                vertices.Add(new VertexPositionNormalTexture()
                {
                    Position = vertexes[i].Position.Value,
                    Normal = vertexes[i].Normal.Value,
                    Texture = vertexes[i].Texture.Value,
                });
            }

            return vertices.ToArray();
        }

        public VertexTypes GetVertexType()
        {
            return VertexTypes.PositionNormalTexture;
        }
        public int GetStride()
        {
            return SizeInBytes;
        }
        public IVertex Convert(Vertex vert)
        {
            return new VertexPositionNormalTexture()
            {
                Position = vert.Position.Value,
                Normal = vert.Normal.Value,
                Texture = vert.Texture.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Normal: {1}; Texture: {2}", this.Position, this.Normal, this.Texture);
        }
    };
}
