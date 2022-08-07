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
    public class PositionColorVsSkinned : IDisposable
    {
        /// <summary>
        /// Global data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSGlobals
        {
            /// <summary>
            /// Animation palette width
            /// </summary>
            public uint AnimationPaletteWidth;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;
        }

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
            /// <summary>
            /// Animation offset 1
            /// </summary>
            public uint AnimationOffset;
            /// <summary>
            /// Animation offset 2
            /// </summary>
            public uint AnimationOffset2;
            /// <summary>
            /// Animation interpolation value
            /// </summary>
            public float AnimationInterpolation;
            public float Pad4;
        }

        /// <summary>
        /// Globals constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSGlobals> vsGlobals;
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
        public PositionColorVsSkinned(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionColor_Skinned_Cso == null;
            var bytes = Resources.Vs_PositionColor_Skinned_Cso ?? Resources.Vs_PositionColor_Skinned;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(PositionColorVsSkinned), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(PositionColorVsSkinned), bytes);
            }

            vsGlobals = new EngineConstantBuffer<VSGlobals>(graphics, nameof(PositionColorVsSkinned) + "." + nameof(VSGlobals));
            vsPerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(PositionColorVsSkinned) + "." + nameof(VSPerFrame));
            vsPerInstance = new EngineConstantBuffer<VSPerInstance>(graphics, nameof(PositionColorVsSkinned) + "." + nameof(VSPerInstance));
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
                Shader?.Dispose();
                vsGlobals?.Dispose();
                vsPerFrame?.Dispose();
                vsPerInstance?.Dispose();
            }
        }

        /// <summary>
        /// Sets global data
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWidth">Animation palette texture width</param>
        public void SetVSGlobals(EngineShaderResourceView animationPalette, uint animationPaletteWidth)
        {
            var data = new VSGlobals
            {
                AnimationPaletteWidth = animationPaletteWidth,
            };

            vsGlobals.WriteData(data);

            Graphics.SetVertexShaderResources(0, 1, animationPalette);
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
        /// <param name="animationOffset">Animation offset 1</param>
        /// <param name="animationOffset2">Animation offset 2</param>
        /// <param name="animationInterpolation">Animation interpolation value</param>
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

            vsPerInstance.WriteData(data);
        }
    }
}
