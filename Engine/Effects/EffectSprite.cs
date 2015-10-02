using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectSprite : Drawer
    {
        /// <summary>
        /// Maximum number of bones in a skeleton
        /// </summary>
        public const int MaxBoneTransforms = 96;

        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EffectTechnique PositionColor = null;
        /// <summary>
        /// Position texture technique
        /// </summary>
        public readonly EffectTechnique PositionTexture = null;
        /// <summary>
        /// Position texture using red channer as gray-scale technique
        /// </summary>
        public readonly EffectTechnique PositionTextureRED = null;
        /// <summary>
        /// Position texture using green channer as gray-scale technique
        /// </summary>
        public readonly EffectTechnique PositionTextureGREEN = null;
        /// <summary>
        /// Position texture using blue channer as gray-scale technique
        /// </summary>
        public readonly EffectTechnique PositionTextureBLUE = null;
        /// <summary>
        /// Position texture using alpha channer as gray-scale technique
        /// </summary>
        public readonly EffectTechnique PositionTextureALPHA = null;
        /// <summary>
        /// Position texture without alpha channel
        /// </summary>
        public readonly EffectTechnique PositionTextureNOALPHA = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// Inverse world matrix effect variable
        /// </summary>
        private EffectMatrixVariable worldInverse = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EffectScalarVariable textureIndex = null;
        /// <summary>
        /// Color effect variable
        /// </summary>
        private EffectVectorVariable color = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable textures = null;

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
        /// Inverse world matrix
        /// </summary>
        protected Matrix WorldInverse
        {
            get
            {
                return this.worldInverse.GetMatrix();
            }
            set
            {
                this.worldInverse.SetMatrix(value);
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
        /// Texture index
        /// </summary>
        protected int TextureIndex
        {
            get
            {
                return (int)this.textureIndex.GetFloat();
            }
            set
            {
                this.textureIndex.Set((float)value);
            }
        }
        /// <summary>
        /// Color
        /// </summary>
        protected Color4 Color
        {
            get
            {
                return new Color4(this.color.GetFloatVector());
            }
            set
            {
                this.color.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected ShaderResourceView Textures
        {
            get
            {
                return this.textures.GetResource();
            }
            set
            {
                this.textures.SetResource(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectSprite(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.PositionColor = this.Effect.GetTechniqueByName("PositionColor");
            this.PositionTexture = this.Effect.GetTechniqueByName("PositionTexture");
            this.PositionTextureNOALPHA = this.Effect.GetTechniqueByName("PositionTextureNOALPHA");
            this.PositionTextureRED = this.Effect.GetTechniqueByName("PositionTextureRED");
            this.PositionTextureGREEN = this.Effect.GetTechniqueByName("PositionTextureGREEN");
            this.PositionTextureBLUE = this.Effect.GetTechniqueByName("PositionTextureBLUE");
            this.PositionTextureALPHA = this.Effect.GetTechniqueByName("PositionTextureALPHA");

            this.AddInputLayout(this.PositionColor, VertexPositionColor.GetInput());
            this.AddInputLayout(this.PositionTexture, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureNOALPHA, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureRED, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureGREEN, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureBLUE, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureALPHA, VertexPositionTexture.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldInverse = this.Effect.GetVariableByName("gWorldInverse").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.textureIndex = this.Effect.GetVariableByName("gTextureIndex").AsScalar();
            this.color = this.Effect.GetVariableByName("gColor").AsVector();
            this.textures = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();
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
                if (vertexType == VertexTypes.PositionColor)
                {
                    return this.PositionColor;
                }
                else if (vertexType == VertexTypes.PositionTexture)
                {
                    return this.PositionTexture;
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
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.World = world;
            this.WorldInverse = Matrix.Invert(world);
            this.WorldViewProjection = world * viewProjection;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="texture">Texture</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            Color4 color,
            ShaderResourceView texture,
            int textureIndex)
        {
            this.Color = color;
            this.Textures = texture;
            this.TextureIndex = textureIndex;
        }
    }
}
