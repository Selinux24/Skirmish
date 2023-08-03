using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine vertex buffer interface
    /// </summary>
    public interface IEngineVertexBuffer : IDisposable
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates the input layout for the buffer
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="signature">Shader signature</param>
        /// <param name="bufferSlot">Buffer slot</param>
        void CreateInputLayout(string name, byte[] signature, int bufferSlot);
        /// <summary>
        /// Sets the vertex buffer in the input assembler
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetVertexBuffers(IEngineDeviceContext dc);
        /// <summary>
        /// Sets the buffer input layout in the input assembler
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetInputLayout(IEngineDeviceContext dc);
        /// <summary>
        /// Binds the buffer to the device's stream output target
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetStreamOutputTargets(IEngineDeviceContext dc);
        /// <summary>
        /// Draws the specified primitive count
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="">Draw count</param>
        void Draw(IEngineDeviceContext dc, int drawCount);
        /// <summary>
        /// Draws auto
        /// </summary>
        /// <param name="dc">Device context</param>
        void DrawAuto(IEngineDeviceContext dc);
    }
}
