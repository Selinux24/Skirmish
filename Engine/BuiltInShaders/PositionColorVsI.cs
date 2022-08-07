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
    public class PositionColorVsI : IDisposable
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
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSPerFrame> vsPerFrame;

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
        public PositionColorVsI(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionColor_I_Cso == null;
            var bytes = Resources.Vs_PositionColor_I_Cso ?? Resources.Vs_PositionColor_I;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(PositionColorVsI), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(PositionColorVsI), bytes);
            }

            vsPerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(PositionColorVsI) + "." + nameof(VSPerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorVsI()
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
    }
}
