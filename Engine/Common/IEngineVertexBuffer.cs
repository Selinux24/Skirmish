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
        /// Binds the buffer to the device's input assembler
        /// </summary>
        /// <param name="topology">Primitive topology</param>
        void SetInputAssembler(Topology topology);
        /// <summary>
        /// Draws the specified primitive count
        /// </summary>
        /// <param name="">Draw count</param>
        void Draw(int drawCount);
    }
}
