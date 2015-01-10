using System;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
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
        /// Cubemap drawing technique
        /// </summary>
        public readonly EffectTechnique Cubemap = null;

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
            this.Cubemap = this.Effect.GetTechniqueByName("Cubemap");

            this.AddInputLayout(this.Cubemap, VertexPosition.GetInput());

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.cubeTexture = this.Effect.GetVariableByName("gCubemap").AsShaderResource();
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
                if (vertexType == VertexTypes.Position)
                {
                    return this.Cubemap;
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
