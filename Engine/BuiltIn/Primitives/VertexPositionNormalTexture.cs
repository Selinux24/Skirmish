using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Primitives
{
    /// <summary>
    /// Position normal texture vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTexture : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public static EngineInputElement[] Input(int slot)
        {
            return
            [
                new ("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
                new ("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
                new ("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="normals">Normals</param>
        /// <param name="uvs">UV coordinates</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static IEnumerable<VertexPositionNormalTexture> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector3> normals, IEnumerable<Vector2> uvs)
        {
            if (vertices.Count() != uvs.Count()) throw new ArgumentException("Vertices and uvs must have the same length");
            if (vertices.Count() != normals.Count()) throw new ArgumentException("Vertices and normals must have the same length");

            var vArray = vertices.ToArray();
            var nArray = normals.ToArray();
            var uvArray = uvs.ToArray();

            VertexPositionNormalTexture[] res = new VertexPositionNormalTexture[vArray.Length];

            for (int i = 0; i < vArray.Length; i++)
            {
                res[i] = new VertexPositionNormalTexture() { Position = vArray[i], Normal = nArray[i], Texture = uvArray[i] };
            }

            return res;
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<IEnumerable<IVertexData>> Convert(IEnumerable<VertexData> vertices)
        {
            var vArray = vertices.ToArray();

            var res = new IVertexData[vArray.Length];

            Parallel.For(0, vArray.Length, (index) =>
            {
                var v = vArray[index];

                res[index] = new VertexPositionNormalTexture
                {
                    Position = v.Position ?? Vector3.Zero,
                    Normal = v.Normal ?? Vector3.Zero,
                    Texture = v.Texture ?? Vector2.Zero
                };
            });

            return await Task.FromResult(res);
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2 Texture;
        /// <summary>
        /// Vertex type
        /// </summary>
        public readonly VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionNormalTexture;
            }
        }

        /// <summary>
        /// Gets if structure contains data for the specified channel
        /// </summary>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns true if structure contains data for the specified channel</returns>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Normal) return true;
            else if (channel == VertexDataChannels.Texture) return true;
            else return false;
        }
        /// <summary>
        /// Gets data channel value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns data for the specified channel</returns>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Normal) return (T)(object)Normal;
            else if (channel == VertexDataChannels.Texture) return (T)(object)Texture;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Normal) Normal = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) Texture = (Vector2)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexPositionNormalTexture));
        }
        /// <summary>
        /// Get input elements
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public readonly EngineInputElement[] GetInput(int slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position}; Normal: {Normal}; Texture: {Texture};";
        }
    };
}
