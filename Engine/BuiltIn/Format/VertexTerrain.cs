using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Format
{
    /// <summary>
    /// Terrain vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexTerrain : IVertexData
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
                new ("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, 32, slot, EngineInputClassification.PerVertexData, 0),
                new ("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 44, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<IEnumerable<VertexTerrain>> Convert(IEnumerable<VertexData> vertices)
        {
            var vArray = vertices.ToArray();

            var res = new VertexTerrain[vArray.Length];

            Parallel.For(0, vArray.Length, (index) =>
            {
                var v = vArray[index];

                res[index] = new VertexTerrain
                {
                    Position = v.Position ?? Vector3.Zero,
                    Normal = v.Normal ?? Vector3.Zero,
                    Texture = v.Texture ?? Vector2.Zero,
                    Tangent = v.Tangent ?? Vector3.UnitX,
                    Color = v.Color ?? Color4.White,
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
        /// Tangent
        /// </summary>
        public Vector3 Tangent;
        /// <summary>
        /// Color
        /// </summary>
        public Color4 Color;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Normal) return true;
            else if (channel == VertexDataChannels.Texture) return true;
            else if (channel == VertexDataChannels.Tangent) return true;
            else if (channel == VertexDataChannels.Color) return true;
            else return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Normal) return (T)(object)Normal;
            else if (channel == VertexDataChannels.Texture) return (T)(object)Texture;
            else if (channel == VertexDataChannels.Tangent) return (T)(object)Tangent;
            else if (channel == VertexDataChannels.Color) return (T)(object)Color;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Normal) Normal = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) Texture = (Vector2)(object)value;
            else if (channel == VertexDataChannels.Tangent) Tangent = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Color) Color = (Color4)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexTerrain));
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(int slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position}; Normal: {Normal}; Texture: {Texture}; Tangent: {Tangent};";
        }
    }
}
