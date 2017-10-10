using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Skinned position normal color vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexSkinnedPositionNormalColor : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public static InputElement[] Input(int slot)
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, slot, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, slot, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 24, slot, InputClassification.PerVertexData, 0),
                new InputElement("WEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32_Float, 40, slot, InputClassification.PerVertexData, 0),
                new InputElement("BONEINDICES", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, 52, slot, InputClassification.PerVertexData, 0 ),
            };
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
        /// Color
        /// </summary>
        public Color4 Color;
        /// <summary>
        /// Weight 1
        /// </summary>
        public float Weight1;
        /// <summary>
        /// Weight 2
        /// </summary>
        public float Weight2;
        /// <summary>
        /// Weight 3
        /// </summary>
        public float Weight3;
        /// <summary>
        /// Bone 1
        /// </summary>
        public byte BoneIndex1;
        /// <summary>
        /// Bone 2
        /// </summary>
        public byte BoneIndex2;
        /// <summary>
        /// Bone 3
        /// </summary>
        public byte BoneIndex3;
        /// <summary>
        /// Bone 4
        /// </summary>
        public byte BoneIndex4;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionNormalColorSkinned;
            }
        }

        /// <summary>
        /// Gets if structure contains data for the specified channel
        /// </summary>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns true if structure contains data for the specified channel</returns>
        public bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Normal) return true;
            else if (channel == VertexDataChannels.Color) return true;
            else if (channel == VertexDataChannels.Weights) return true;
            else if (channel == VertexDataChannels.BoneIndices) return true;
            else return false;
        }
        /// <summary>
        /// Gets data channel value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns data for the specified channel</returns>
        public T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)this.Position;
            else if (channel == VertexDataChannels.Normal) return (T)(object)this.Normal;
            else if (channel == VertexDataChannels.Color) return (T)(object)this.Color;
            else if (channel == VertexDataChannels.Weights) return (T)(object)(new[] { this.Weight1, this.Weight2, this.Weight3, (1.0f - this.Weight1 - this.Weight2 - this.Weight3) });
            else if (channel == VertexDataChannels.BoneIndices) return (T)(object)(new[] { this.BoneIndex1, this.BoneIndex2, this.BoneIndex3, this.BoneIndex4 });
            else throw new EngineException(string.Format("Channel data not found: {0}", channel));
        }
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) this.Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Normal) this.Normal = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Color) this.Color = (Color4)(object)value;
            else if (channel == VertexDataChannels.Weights)
            {
                float[] weights = (float[])(object)value;

                this.Weight1 = weights[0];
                this.Weight2 = weights[1];
                this.Weight3 = weights[2];
            }
            else if (channel == VertexDataChannels.BoneIndices)
            {
                byte[] boneIndices = (byte[])(object)value;

                this.BoneIndex1 = boneIndices[0];
                this.BoneIndex2 = boneIndices[1];
                this.BoneIndex3 = boneIndices[2];
                this.BoneIndex4 = boneIndices[3];
            }
            else throw new EngineException(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexSkinnedPositionNormalColor));
        }
        /// <summary>
        /// Get input elements
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public InputElement[] GetInput(int slot)
        {
            return Input(slot);
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Skinned; Position: {0} Normal: {1}; Color: {2}", this.Position, this.Normal, this.Color);
        }
    };
}
