using Engine.Common;
using Engine.Mathematics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Format
{
    using Format = Vortice.DXGI.Format;

    /// <summary>
    /// Position vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <returns></returns>
        public static EngineInputElement[] Input(uint slot)
        {
            return
            [
                new ("POSITION", 0, Format.R32G32B32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="uvs">Uv texture coordinates</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static VertexPosition[] Generate(Vector3[] vertices)
        {
            VertexPosition[] res = new VertexPosition[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                res[i] = new VertexPosition() { Position = vertices[i] };
            }

            return res;
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<VertexPosition[]> Convert(VertexData[] vertices)
        {
            var res = new VertexPosition[vertices.Length];

            Parallel.For(0, vertices.Length, (index) =>
            {
                var v = vertices[index];

                res[index] = new VertexPosition
                {
                    Position = v.Position ?? Vector3.Zero,
                };
            });

            return await Task.FromResult(res);
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<VertexPosition>();
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(uint slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position};";
        }
    }
}
