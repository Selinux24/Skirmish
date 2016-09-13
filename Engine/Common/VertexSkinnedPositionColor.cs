using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    /// <summary>
    /// Skinned position color vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexSkinnedPositionColor : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <returns></returns>
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("WEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32_Float, 28, 0, InputClassification.PerVertexData, 0),
                new InputElement("BONEINDICES", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, 40, 0, InputClassification.PerVertexData, 0 ),
           };
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
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
                return VertexTypes.PositionColorSkinned;
            }
        }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexSkinnedPositionColor));
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
            if (channel == VertexDataChannels.Position) return this.Position.Cast<T>();
            else if (channel == VertexDataChannels.Color) return this.Color.Cast<T>();
            else if (channel == VertexDataChannels.Weights) return (new[] { this.Weight1, this.Weight2, this.Weight3, (1.0f - this.Weight1 - this.Weight2 - this.Weight3) }).Cast<T>();
            else if (channel == VertexDataChannels.BoneIndices) return (new[] { this.BoneIndex1, this.BoneIndex2, this.BoneIndex3, this.BoneIndex4 }).Cast<T>();
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) this.Position = value.Cast<Vector3>();
            else if (channel == VertexDataChannels.Color) this.Color = value.Cast<Color4>();
            else if (channel == VertexDataChannels.Weights)
            {
                float[] weights = value.Cast<float[]>();

                this.Weight1 = weights[0];
                this.Weight2 = weights[1];
                this.Weight3 = weights[2];
            }
            else if (channel == VertexDataChannels.BoneIndices)
            {
                byte[] boneIndices = value.Cast<byte[]>();

                this.BoneIndex1 = boneIndices[0];
                this.BoneIndex2 = boneIndices[1];
                this.BoneIndex3 = boneIndices[2];
                this.BoneIndex4 = boneIndices[3];
            }
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Skinned; Position: {0}; Color: {1}", this.Position, this.Color);
        }
    };
}
