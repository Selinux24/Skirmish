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
        void SetVertexBuffers(EngineDeviceContext dc);
        /// <summary>
        /// Sets the buffer input layout in the input assembler
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetInputLayout(EngineDeviceContext dc);
        /// <summary>
        /// Binds the buffer to the device's stream output target
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetStreamOutputTargets(EngineDeviceContext dc);
        /// <summary>
        /// Draws the specified primitive count
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="">Draw count</param>
        void Draw(EngineDeviceContext dc, int drawCount);
        /// <summary>
        /// Draws auto
        /// </summary>
        /// <param name="dc">Device context</param>
        void DrawAuto(EngineDeviceContext dc);
    }
}
