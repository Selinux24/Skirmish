using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.FmtObj
{
    /// <summary>
    /// Face
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="items">Item list</param>
    struct Face(uint[] items)
    {
        /// <summary>
        /// Position
        /// </summary>
        public uint Position { get; set; } = items.Length > 0 ? items[0] : 0;
        /// <summary>
        /// Texture UV map
        /// </summary>
        public uint UV { get; set; } = items.Length > 1 ? items[1] : 0;
        /// <summary>
        /// Normal
        /// </summary>
        public uint Normal { get; set; } = items.Length > 2 ? items[2] : 0;

        /// <summary>
        /// Gets the position index
        /// </summary>
        /// <param name="offset">Index offset</param>
        /// <returns>Returns the based 0 index</returns>
        public readonly int GetPositionIndex(int offset)
        {
            return (int)Position - 1 - offset;
        }
        /// <summary>
        /// Gets the Uv index
        /// </summary>
        /// <param name="offset">Index offset</param>
        /// <returns>Returns the based 0 index</returns>
        public readonly int? GetUVIndex(int offset)
        {
            return UV != 0 ? (int)UV - 1 - offset : null;
        }
        /// <summary>
        /// Gets the normal index
        /// </summary>
        /// <param name="offset">Index offset</param>
        /// <returns>Returns the based 0 index</returns>
        public readonly int? GetNormalIndex(int offset)
        {
            return Normal != 0 ? (int)Normal - 1 - offset : null;
        }

        /// <summary>
        /// Creates a vertex
        /// </summary>
        /// <param name="faceIndex">Face index</param>
        /// <param name="vertexIndex">Vertex index</param>
        /// <param name="points">Point list</param>
        /// <param name="uvs">Uvs</param>
        /// <param name="normals">Normals</param>
        /// <param name="offset">Offset index</param>
        public VertexData CreateVertex(int faceIndex, int vertexIndex, IEnumerable<Vector3> points, IEnumerable<Vector2> uvs, IEnumerable<Vector3> normals, int offset)
        {
            int vIndex = GetPositionIndex(offset);
            int? uvIndex = GetUVIndex(offset);
            int? nmIndex = GetNormalIndex(offset);

            return new VertexData
            {
                Position = points.ElementAt(vIndex),
                Texture = uvIndex >= 0 ? uvs.ElementAt(uvIndex.Value) : null,
                Normal = nmIndex >= 0 ? normals.ElementAt(nmIndex.Value) : null,
                FaceIndex = faceIndex,
                VertexIndex = vertexIndex
            };
        }

        /// <summary>
        /// Gets the text representation of the face
        /// </summary>
        public override readonly string ToString()
        {
            return $"{Position}/{UV}/{Normal}";
        }
    }
}
