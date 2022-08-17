using System.Runtime.InteropServices;
using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Global resources
    /// </summary>
    public class ResourcesVSGlobal : IDisposable
    {
        /// <summary>
        /// Global data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct VSGlobals : IBufferData
        {
            /// <summary>
            /// Material palette width
            /// </summary>
            [FieldOffset(0)]
            public uint MaterialPaletteWidth;
            /// <summary>
            /// Animation palette width
            /// </summary>
            [FieldOffset(4)]
            public uint AnimationPaletteWidth;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSGlobals));
            }
        }

        /// <summary>
        /// Globals constant buffer
        /// </summary>
        public EngineConstantBuffer<VSGlobals> Globals { get; private set; }
        /// <summary>
        /// Material palette resource view
        /// </summary>
        public EngineShaderResourceView MaterialPalette { get; private set; }
        /// <summary>
        /// Animation palette resource view
        /// </summary>
        public EngineShaderResourceView AnimationPalette { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ResourcesVSGlobal(Graphics graphics)
        {
            Globals = new EngineConstantBuffer<VSGlobals>(graphics, nameof(ResourcesVSGlobal) + "." + nameof(VSGlobals));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ResourcesVSGlobal()
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
                Globals?.Dispose();
            }
        }

        /// <summary>
        /// Sets global data
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWidth">Animation palette texture width</param>
        public void SetCBGlobals(EngineShaderResourceView materialPalette, uint materialPaletteWidth, EngineShaderResourceView animationPalette, uint animationPaletteWidth)
        {
            MaterialPalette = materialPalette;
            AnimationPalette = animationPalette;

            var data = new VSGlobals
            {
                MaterialPaletteWidth = materialPaletteWidth,
                AnimationPaletteWidth = animationPaletteWidth,
            };
            Globals.WriteData(data);
        }
    }
}
