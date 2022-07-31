using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class PositionColorVsSkinnedI : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VSGlobals
        {
            public uint AnimationPaletteWidth;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerFrame
        {
            public Matrix World;
            public Matrix WorldViewProjection;
        }

        private readonly EngineVertexShader shader;
        private readonly Buffer vsGlobals;
        private readonly Buffer vsPerFrame;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionColorVsSkinnedI(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionColor_Skinned_I_Cso == null;
            var bytes = Resources.Vs_PositionColor_Skinned_I_Cso ?? Resources.Vs_PositionColor_Skinned_I;
            if (compile)
            {
                shader = graphics.CompileVertexShader(nameof(PositionColorVsSkinnedI), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                shader = graphics.LoadVertexShader(nameof(PositionColorVsSkinnedI), bytes);
            }

            vsGlobals = graphics.CreateConstantBuffer<VSGlobals>(nameof(PositionColorVsSkinnedI) + "." + nameof(VSGlobals));
            vsPerFrame = graphics.CreateConstantBuffer<VSPerFrame>(nameof(PositionColorVsSkinnedI) + "." + nameof(VSPerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorVsSkinnedI()
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
                shader?.Dispose();
                vsGlobals?.Dispose();
                vsPerFrame?.Dispose();
            }
        }


        public void SetVSGlobals(uint animationPaletteWidth)
        {
            var data = new VSGlobals
            {
                AnimationPaletteWidth = animationPaletteWidth,
            };

            Graphics.WriteDiscardBuffer(vsGlobals, data);
        }
        public void SetVSPerFrame(Matrix world, Matrix worldViewProjection)
        {
            var data = new VSPerFrame
            {
                World = Matrix.Transpose(world),
                WorldViewProjection = Matrix.Transpose(worldViewProjection),
            };

            Graphics.WriteDiscardBuffer(vsPerFrame, data);
        }
        public void SetShader()
        {
            Graphics.SetVertexShader(shader);
        }
        public void Draw(Mesh mesh, BufferManager bufferManager)
        {
            bufferManager.SetIndexBuffer(mesh.IndexBuffer);
            bufferManager.SetInputAssembler(mesh.VertexBuffer, mesh.Topology);

            mesh.Draw(Graphics);
        }
    }
}
