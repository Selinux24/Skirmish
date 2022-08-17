using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Per-frame resources
    /// </summary>
    public class ResourcesVSPerFrame : IDisposable
    {
        /// <summary>
        /// Per-frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 128)]
        public struct VSPerFrame : IBufferData
        {
            /// <summary>
            /// World matrix
            /// </summary>
            [FieldOffset(0)]
            public Matrix World;
            /// <summary>
            /// World view projection matrix
            /// </summary>
            [FieldOffset(64)]
            public Matrix WorldViewProjection;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSPerFrame));
            }
        }

        /// <summary>
        /// Globals constant buffer
        /// </summary>
        public EngineConstantBuffer<VSPerFrame> PerFrame { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ResourcesVSPerFrame(Graphics graphics)
        {
            PerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(ResourcesVSPerFrame) + "." + nameof(VSPerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ResourcesVSPerFrame()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                PerFrame?.Dispose();
            }
        }

        /// <summary>
        /// Sets global data
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWidth">Animation palette texture width</param>
        public void SetCBPerFrame(Matrix world, Matrix worldViewProjection)
        {
            var data = new VSPerFrame
            {
                World = Matrix.Transpose(world),
                WorldViewProjection = Matrix.Transpose(worldViewProjection),
            };
            PerFrame.WriteData(data);
        }
    }
}
