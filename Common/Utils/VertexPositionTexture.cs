using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionTexture : IVertex
    {
        public Vector3 Position;
        public Vector2 Texture;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionTexture));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexPositionTexture[] Create(Vector3[] positions, Vector3[] normals, Vector2[] uvs)
        {
            List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();

            for (int i = 0; i < positions.Length; i++)
            {
                vertices.Add(new VertexPositionTexture()
                {
                    Position = positions[i],
                    Texture = uvs[i],
                });
            }

            return vertices.ToArray();
        }
        public static VertexPositionTexture[] Create(Vertex[] vertexes)
        {
            List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();

            for (int i = 0; i < vertexes.Length; i++)
            {
                vertices.Add(new VertexPositionTexture()
                {
                    Position = vertexes[i].Position.Value,
                    Texture = vertexes[i].Texture.Value,
                });
            }

            return vertices.ToArray();
        }

        public VertexTypes GetVertexType()
        {
            return VertexTypes.PositionTexture;
        }
        public int GetStride()
        {
            return SizeInBytes;
        }
        public IVertex Convert(Vertex vert)
        {
            return new VertexPositionTexture()
            {
                Position = vert.Position.Value,
                Texture = vert.Texture.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Texture: {1}", this.Position, this.Texture);
        }
    };
}
