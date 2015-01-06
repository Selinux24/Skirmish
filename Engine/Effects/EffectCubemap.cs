using System;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectPassDescription = SharpDX.Direct3D11.EffectPassDescription;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using InputElement = SharpDX.Direct3D11.InputElement;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Properties;

    /// <summary>
    /// Cube map effect
    /// </summary>
    public class EffectCubemap : Drawer
    {
        #region Buffers

        /// <summary>
        /// Per frame update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public Matrix WorldViewProjection;

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
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable cubeTexture = null;

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
        /// Texture
        /// </summary>
        protected ShaderResourceView CubeTexture
        {
            get
            {
                return this.cubeTexture.GetResource();
            }
            set
            {
                this.cubeTexture.SetResource(value);
            }
        }

        /// <summary>
        /// Per frame buffer structure
        /// </summary>
        public EffectCubemap.PerFrameBuffer FrameBuffer = new EffectCubemap.PerFrameBuffer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        public EffectCubemap(Device device)
            : base(device, Resources.ShaderCubemap)
        {
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.cubeTexture = this.Effect.GetVariableByName("gCubemap").AsShaderResource();
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

            if (vertexType == VertexTypes.Position)
            {
                technique = "Cubemap";

                InputElement[] input = VertexPosition.GetInput();

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
        public void UpdatePerFrame()
        {
            this.WorldViewProjection = this.FrameBuffer.WorldViewProjection;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(ShaderResourceView cubeTexture)
        {
            this.CubeTexture = cubeTexture;
        }
    }
}
