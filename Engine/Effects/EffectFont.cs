using System;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Properties;

    /// <summary>
    /// Font effect
    /// </summary>
    public class EffectFont : Drawer
    {
        #region Buffers

        /// <summary>
        /// Per frame update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public Matrix World;
            public Matrix WorldViewProjection;
            public Color4 Color;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerFrameBuffer));
                }
            }
        }

        #endregion

        /// <summary>
        /// Font drawing technique
        /// </summary>
        public readonly EffectTechnique FontDrawer = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Color effect variable
        /// </summary>
        private EffectVectorVariable color = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable texture = null;

        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return this.world.GetMatrix();
            }
            set
            {
                this.world.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjection.GetMatrix();
            }
            set
            {
                this.worldViewProjection.SetMatrix(value);
            }
        }
        /// <summary>
        /// Color
        /// </summary>
        protected Color4 Color
        {
            get
            {
                return this.color.GetVector<Color4>();
            }
            set
            {
                this.color.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected ShaderResourceView Texture
        {
            get
            {
                return this.texture.GetResource();
            }
            set
            {
                this.texture.SetResource(value);
            }
        }

        /// <summary>
        /// Per frame buffer structure
        /// </summary>
        public EffectFont.PerFrameBuffer FrameBuffer = new EffectFont.PerFrameBuffer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        public EffectFont(Device device)
            : base(device, Resources.ShaderFont)
        {
            this.FontDrawer = this.Effect.GetTechniqueByName("FontDrawer");

            this.AddInputLayout(this.FontDrawer, VertexPositionTexture.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.color = this.Effect.GetVariableByName("gColor").AsVector();
            this.texture = this.Effect.GetVariableByName("gTexture").AsShaderResource();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="stage">Stage</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, DrawingStages stage)
        {
            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.PositionTexture)
                {
                    return this.FontDrawer;
                }
                else
                {
                    throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                }
            }
            else
            {
                throw new Exception(string.Format("Bad stage for effect: {0}", stage));
            }
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        public void UpdatePerFrame(ShaderResourceView texture)
        {
            this.World = this.FrameBuffer.World;
            this.WorldViewProjection = this.FrameBuffer.WorldViewProjection;
            this.Color = this.FrameBuffer.Color;
            this.Texture = texture;
        }
    }
}
