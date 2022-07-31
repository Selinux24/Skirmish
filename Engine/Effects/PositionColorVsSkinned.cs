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
    public class PositionColorVsSkinned : IDisposable
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

        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerInstance
        {
            public Color4 TintColor;
            public uint MaterialIndex;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;
            public uint AnimationOffset;
            public uint AnimationOffset2;
            public float AnimationInterpolation;
            public float Pad4;
        }

        private readonly EngineVertexShader shader;
        private readonly Buffer vsGlobals;
        private readonly Buffer vsPerFrame;
        private readonly Buffer vsPerInstance;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionColorVsSkinned(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionColor_Skinned_Cso == null;
            var bytes = Resources.Vs_PositionColor_Skinned_Cso ?? Resources.Vs_PositionColor_Skinned;
            if (compile)
            {
                shader = graphics.CompileVertexShader(nameof(PositionColorVsSkinned), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                shader = graphics.LoadVertexShader(nameof(PositionColorVsSkinned), bytes);
            }

            vsGlobals = graphics.CreateConstantBuffer<VSGlobals>(nameof(PositionColorVsSkinned) + "." + nameof(VSGlobals));
            vsPerFrame = graphics.CreateConstantBuffer<VSPerFrame>(nameof(PositionColorVsSkinned) + "." + nameof(VSPerFrame));
            vsPerInstance = graphics.CreateConstantBuffer<VSPerInstance>(nameof(PositionColorVsSkinned) + "." + nameof(VSPerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorVsSkinned()
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
                vsPerInstance?.Dispose();
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
        public void SetVSPerInstance(Color4 tintColor, uint materialIndex, uint animationOffset, uint animationOffset2, float animationInterpolation)
        {
            var data = new VSPerInstance
            {
                TintColor = tintColor,
                MaterialIndex = materialIndex,
                AnimationOffset = animationOffset,
                AnimationOffset2 = animationOffset2,
                AnimationInterpolation = animationInterpolation,
            };

            Graphics.WriteDiscardBuffer(vsPerInstance, data);
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
