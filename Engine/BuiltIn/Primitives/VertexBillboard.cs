﻿using Engine.Common;
using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Primitives
{
    /// <summary>
    /// Billboard vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexBillboard : IVertexData
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
                new ("SIZE", 0, SharpDX.DXGI.Format.R32G32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Sprite size
        /// </summary>
        public Vector2 Size;
        /// <summary>
        /// Vertex type
        /// </summary>
        public readonly VertexTypes VertexType
        {
            get
            {
                return VertexTypes.Billboard;
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
            else if (channel == VertexDataChannels.Size) return true;
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
            else if (channel == VertexDataChannels.Size) return (T)(object)Size;
            else throw new EngineException($"Channel data not found: {channel};");
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
            else if (channel == VertexDataChannels.Size) Size = (Vector2)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexBillboard));
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
            return $"Position: {Position}; Size: {Size};";
        }
    };
}
