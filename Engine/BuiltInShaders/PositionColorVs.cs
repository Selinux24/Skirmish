using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInShaders
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class PositionColorVs : IDisposable
    {
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerFrame
        {
            /// <summary>
            /// World matrix
            /// </summary>
            public Matrix World;
            /// <summary>
            /// World view projection matrix
            /// </summary>
            public Matrix WorldViewProjection;
        }

        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerInstance
        {
            /// <summary>
            /// Tint color
            /// </summary>
            public Color4 TintColor;
            /// <summary>
            /// Material index
            /// </summary>
            public uint MaterialIndex;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;
        }

        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSPerFrame> vsPerFrame;
        /// <summary>
        /// Per instance constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSPerInstance> vsPerInstance;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EngineVertexShader Shader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionColorVs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionColor_Cso == null;
            var bytes = Resources.Vs_PositionColor_Cso ?? Resources.Vs_PositionColor;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(PositionColorVs), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(PositionColorVs), bytes);
            }

            vsPerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(PositionColorVs) + "." + nameof(VSPerFrame));
            vsPerInstance = new EngineConstantBuffer<VSPerInstance>(graphics, nameof(PositionColorVs) + "." + nameof(VSPerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorVs()
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
                Shader?.Dispose();
                vsPerFrame?.Dispose();
                vsPerInstance?.Dispose();
            }
        }

        /// <summary>
        /// Sets per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="worldViewProjection">World view projection matrix</param>
        public void SetVSPerFrame(Matrix world, Matrix worldViewProjection)
        {
            var data = new VSPerFrame
            {
                World = Matrix.Transpose(world),
                WorldViewProjection = Matrix.Transpose(worldViewProjection),
            };

            vsPerFrame.WriteData(data);
        }
        /// <summary>
        /// Sets per instance data
        /// </summary>
        /// <param name="tintColor">Tint color</param>
        /// <param name="materialIndex">Material index</param>
        public void SetVSPerInstance(Color4 tintColor, uint materialIndex)
        {
            var data = new VSPerInstance
            {
                TintColor = tintColor,
                MaterialIndex = materialIndex,
            };

            vsPerInstance.WriteData(data);
        }
    }
}
