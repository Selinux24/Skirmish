using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Per-frame resources
    /// </summary>
    public class ResourcesPSPerFrameNoLit : IDisposable
    {
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        public struct PSPerFrame : IBufferData
        {
            /// <summary>
            /// Eye position world
            /// </summary>
            [FieldOffset(0)]
            public Vector3 EyePositionWorld;

            /// <summary>
            /// Fog color
            /// </summary>
            [FieldOffset(16)]
            public Color4 FogColor;

            /// <summary>
            /// Fog start distance
            /// </summary>
            [FieldOffset(32)]
            public float FogStart;
            /// <summary>
            /// Fog range distance
            /// </summary>
            [FieldOffset(36)]
            public float FogRange;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PSPerFrame));
            }
        }

        /// <summary>
        /// Globals constant buffer
        /// </summary>
        public EngineConstantBuffer<PSPerFrame> PerFrame { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ResourcesPSPerFrameNoLit(Graphics graphics)
        {
            PerFrame = new EngineConstantBuffer<PSPerFrame>(graphics, nameof(ResourcesPSPerFrameNoLit) + "." + nameof(PSPerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ResourcesPSPerFrameNoLit()
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
        /// Sets per frame data
        /// </summary>
        /// <param name="eyePositionWorld">Eye position world</param>
        /// <param name="lights">Scene lights</param>
        public void SetCBPerFrame(Vector3 eyePositionWorld, SceneLights lights)
        {
            var data = new PSPerFrame
            {
                EyePositionWorld = eyePositionWorld,

                FogColor = lights?.FogColor ?? Color.Transparent,

                FogStart = lights?.FogStart ?? 0,
                FogRange = lights?.FogRange ?? 0,
            };
            PerFrame.WriteData(data);
        }
    }
}
