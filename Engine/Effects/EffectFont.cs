using System;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectPassDescription = SharpDX.Direct3D11.EffectPassDescription;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using InputElement = SharpDX.Direct3D11.InputElement;
using InputLayout = SharpDX.Direct3D11.InputLayout;
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
            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.color = this.Effect.GetVariableByName("gColor").AsVector();
            this.texture = this.Effect.GetVariableByName("gTexture").AsShaderResource();
        }
        /// <summary>
        /// Finds technique and input layout for vertex type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <returns>Returns technique name for specified vertex type</returns>
        public override string AddVertexType(VertexTypes vertexType)
        {
            string technique = null;
            InputLayout layout = null;

            if (vertexType == VertexTypes.PositionTexture)
            {
                technique = "FontDrawer";

                InputElement[] input = VertexPositionTexture.GetInput();

                EffectPassDescription desc = Effect.GetTechniqueByName(technique).GetPassByIndex(0).Description;

                layout = new InputLayout(
                    this.Device,
                    desc.Signature,
                    input);

                this.AddInputLayout(technique, layout);

                return technique;
            }
            else
            {
                throw new Exception("VertexType unknown");
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
