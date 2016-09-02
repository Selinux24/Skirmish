using System;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectBasic : Drawer
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
        /// Position color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalColorSkinned = null;
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
        /// Position texture skinned technique
        /// </summary>
        public readonly EffectTechnique PositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture with normal mapping technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture skinned with normal mapping technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionTexture = null;
        /// <summary>
        /// Position texture using red channer as gray-scale technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionTextureRED = null;
        /// <summary>
        /// Position texture using green channer as gray-scale technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionTextureGREEN = null;
        /// <summary>
        /// Position texture using blue channer as gray-scale technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionTextureBLUE = null;
        /// <summary>
        /// Position texture using alpha channer as gray-scale technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionTextureALPHA = null;
        /// <summary>
        /// Position texture without alpha channel
        /// </summary>
        public readonly EffectTechnique InstancingPositionTextureNOALPHA = null;
        /// <summary>
        /// Position texture skinned technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture with normal mapping technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture skinned with normal mapping technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private EffectVariable dirLights = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private EffectVariable pointLights = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EffectVariable spotLights = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private EffectScalarVariable fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private EffectScalarVariable fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private EffectVectorVariable fogColor = null;
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
        /// From light View * Projection transform
        /// </summary>
        private EffectMatrixVariable fromLightViewProjection = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EffectScalarVariable textureIndex = null;
        /// <summary>
        /// Material effect variable
        /// </summary>
        private EffectVariable material = null;
        /// <summary>
        /// Bone transformation matrices effect variable
        /// </summary>
        private EffectMatrixVariable boneTransforms = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable textures = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EffectShaderResourceVariable normalMap = null;
        /// <summary>
        /// Static shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMapStatic = null;
        /// <summary>
        /// Dynamic shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMapDynamic = null;

        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                using (DataStream ds = this.dirLights.GetRawValue(default(BufferDirectionalLight).Stride * BufferDirectionalLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferDirectionalLight>(BufferDirectionalLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferDirectionalLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.dirLights.SetRawValue(ds, default(BufferDirectionalLight).Stride * BufferDirectionalLight.MAX);
                }
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferPointLight[] PointLights
        {
            get
            {
                using (DataStream ds = this.pointLights.GetRawValue(default(BufferPointLight).Stride * BufferPointLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferPointLight>(BufferPointLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferPointLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.pointLights.SetRawValue(ds, default(BufferPointLight).Stride * BufferPointLight.MAX);
                }
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight[] SpotLights
        {
            get
            {
                using (DataStream ds = this.spotLights.GetRawValue(default(BufferSpotLight).Stride * BufferSpotLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferSpotLight>(BufferSpotLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferSpotLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.spotLights.SetRawValue(ds, default(BufferSpotLight).Stride * BufferSpotLight.MAX);
                }
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                Vector4 v = this.eyePositionWorld.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.eyePositionWorld.Set(v4);
            }
        }
        /// <summary>
        /// Fog start distance
        /// </summary>
        protected float FogStart
        {
            get
            {
                return this.fogStart.GetFloat();
            }
            set
            {
                this.fogStart.Set(value);
            }
        }
        /// <summary>
        /// Fog range distance
        /// </summary>
        protected float FogRange
        {
            get
            {
                return this.fogRange.GetFloat();
            }
            set
            {
                this.fogRange.Set(value);
            }
        }
        /// <summary>
        /// Fog color
        /// </summary>
        protected Color4 FogColor
        {
            get
            {
                return new Color4(this.fogColor.GetFloatVector());
            }
            set
            {
                this.fogColor.Set(value);
            }
        }
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
        /// From light View * Projection transform
        /// </summary>
        protected Matrix FromLightViewProjection
        {
            get
            {
                return this.fromLightViewProjection.GetMatrix();
            }
            set
            {
                this.fromLightViewProjection.SetMatrix(value);
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
        /// Material
        /// </summary>
        protected BufferMaterials Material
        {
            get
            {
                using (DataStream ds = this.material.GetRawValue(default(BufferMaterials).Stride))
                {
                    ds.Position = 0;

                    return ds.Read<BufferMaterials>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferMaterials>(new BufferMaterials[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.material.SetRawValue(ds, default(BufferMaterials).Stride);
                }
            }
        }
        /// <summary>
        /// Bone transformations
        /// </summary>
        protected Matrix[] BoneTransforms
        {
            get
            {
                return this.boneTransforms.GetMatrixArray<Matrix>(MaxBoneTransforms);
            }
            set
            {
                if (value != null && value.Length > MaxBoneTransforms) throw new Exception(string.Format("Bonetransforms must set {0}. Has {1}", MaxBoneTransforms, value.Length));

                if (value == null)
                {
                    this.boneTransforms.SetMatrix(new Matrix[MaxBoneTransforms]);
                }
                else
                {
                    this.boneTransforms.SetMatrix(value);
                }
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
        /// Normal map
        /// </summary>
        protected ShaderResourceView NormalMap
        {
            get
            {
                return this.normalMap.GetResource();
            }
            set
            {
                this.normalMap.SetResource(value);
            }
        }
        /// <summary>
        /// Static shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapStatic
        {
            get
            {
                return this.shadowMapStatic.GetResource();
            }
            set
            {
                this.shadowMapStatic.SetResource(value);
            }
        }
        /// <summary>
        /// Dynamic shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapDynamic
        {
            get
            {
                return this.shadowMapDynamic.GetResource();
            }
            set
            {
                this.shadowMapDynamic.SetResource(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectBasic(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.PositionColor = this.Effect.GetTechniqueByName("PositionColor");
            this.PositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinned");
            this.PositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColor");
            this.PositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinned");
            this.PositionTexture = this.Effect.GetTechniqueByName("PositionTexture");
            this.PositionTextureNOALPHA = this.Effect.GetTechniqueByName("PositionTextureNOALPHA");
            this.PositionTextureRED = this.Effect.GetTechniqueByName("PositionTextureRED");
            this.PositionTextureGREEN = this.Effect.GetTechniqueByName("PositionTextureGREEN");
            this.PositionTextureBLUE = this.Effect.GetTechniqueByName("PositionTextureBLUE");
            this.PositionTextureALPHA = this.Effect.GetTechniqueByName("PositionTextureALPHA");
            this.PositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinned");
            this.PositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTexture");
            this.PositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinned");
            this.PositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangent");
            this.PositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinned");
            this.InstancingPositionColor = this.Effect.GetTechniqueByName("PositionColorI");
            this.InstancingPositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinnedI");
            this.InstancingPositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColorI");
            this.InstancingPositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinnedI");
            this.InstancingPositionTexture = this.Effect.GetTechniqueByName("PositionTextureI");
            this.InstancingPositionTextureNOALPHA = this.Effect.GetTechniqueByName("PositionTextureNOALPHAI");
            this.InstancingPositionTextureRED = this.Effect.GetTechniqueByName("PositionTextureREDI");
            this.InstancingPositionTextureGREEN = this.Effect.GetTechniqueByName("PositionTextureGREENI");
            this.InstancingPositionTextureBLUE = this.Effect.GetTechniqueByName("PositionTextureBLUEI");
            this.InstancingPositionTextureALPHA = this.Effect.GetTechniqueByName("PositionTextureALPHAI");
            this.InstancingPositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinnedI");
            this.InstancingPositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTextureI");
            this.InstancingPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinnedI");
            this.InstancingPositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangentI");
            this.InstancingPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinnedI");

            this.AddInputLayout(this.PositionColor, VertexPositionColor.GetInput());
            this.AddInputLayout(this.PositionColorSkinned, VertexSkinnedPositionColor.GetInput());
            this.AddInputLayout(this.PositionNormalColor, VertexPositionNormalColor.GetInput());
            this.AddInputLayout(this.PositionNormalColorSkinned, VertexSkinnedPositionNormalColor.GetInput());
            this.AddInputLayout(this.PositionTexture, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureNOALPHA, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureRED, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureGREEN, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureBLUE, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureALPHA, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureSkinned, VertexSkinnedPositionTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTexture, VertexPositionNormalTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput());
            this.AddInputLayout(this.PositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput());
            this.AddInputLayout(this.InstancingPositionColor, VertexPositionColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionColorSkinned, VertexSkinnedPositionColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalColor, VertexPositionNormalColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalColorSkinned, VertexSkinnedPositionNormalColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTexture, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureNOALPHA, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureRED, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureGREEN, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureBLUE, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureALPHA, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureSkinned, VertexSkinnedPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTexture, VertexPositionNormalTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput().Merge(VertexInstancingData.GetInput()));

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldInverse = this.Effect.GetVariableByName("gWorldInverse").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.fromLightViewProjection = this.Effect.GetVariableByName("gLightViewProjection").AsMatrix();
            this.textureIndex = this.Effect.GetVariableByName("gTextureIndex").AsScalar();
            this.material = this.Effect.GetVariableByName("gMaterial");
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLights = this.Effect.GetVariableByName("gPointLights");
            this.spotLights = this.Effect.GetVariableByName("gSpotLights");
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.boneTransforms = this.Effect.GetVariableByName("gBoneTransforms").AsMatrix();
            this.textures = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();
            this.normalMap = this.Effect.GetVariableByName("gNormalMap").AsShaderResource();
            this.shadowMapStatic = this.Effect.GetVariableByName("gShadowMapStatic").AsShaderResource();
            this.shadowMapDynamic = this.Effect.GetVariableByName("gShadowMapDynamic").AsShaderResource();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            if (stage == DrawingStages.Drawing)
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return instanced ? this.InstancingPositionColor : this.PositionColor;
                    case VertexTypes.PositionTexture:
                        return instanced ? this.InstancingPositionTexture : this.PositionTexture;
                    case VertexTypes.PositionNormalColor:
                        return instanced ? this.InstancingPositionNormalColor : this.PositionNormalColor;
                    case VertexTypes.PositionNormalTexture:
                        return instanced ? this.InstancingPositionNormalTexture : this.PositionNormalTexture;
                    case VertexTypes.PositionNormalTextureTangent:
                        return instanced ? this.InstancingPositionNormalTextureTangent : this.PositionNormalTextureTangent;
                    case VertexTypes.PositionColorSkinned:
                        return instanced ? this.InstancingPositionColorSkinned : this.PositionColorSkinned;
                    case VertexTypes.PositionTextureSkinned:
                        return instanced ? this.InstancingPositionTextureSkinned : this.PositionTextureSkinned;
                    case VertexTypes.PositionNormalColorSkinned:
                        return instanced ? this.InstancingPositionNormalColorSkinned : this.PositionNormalColorSkinned;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return instanced ? this.InstancingPositionNormalTextureSkinned : this.PositionNormalTextureSkinned;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return instanced ? this.InstancingPositionNormalTextureTangentSkinned : this.PositionNormalTextureTangentSkinned;
                    default:
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
            this.UpdatePerFrame(world, viewProjection, Vector3.Zero, new BoundingFrustum(), null, null, null, Matrix.Identity);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="viewFrustum">Camera frustum</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMapStatic">Static shadow map texture</param>
        /// <param name="shadowMapDynamic">Dynamic shadow map texture</param>
        /// <param name="fromLightViewProjection">From light View * Projection transform</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            BoundingFrustum viewFrustum,
            SceneLights lights,
            ShaderResourceView shadowMapStatic,
            ShaderResourceView shadowMapDynamic,
            Matrix fromLightViewProjection)
        {
            this.World = world;
            this.WorldInverse = Matrix.Invert(world);
            this.WorldViewProjection = world * viewProjection;

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                var dirLights = lights.GetVisibleDirectionalLights(viewFrustum);
                var pointLights = lights.GetVisiblePointLights(viewFrustum);
                var spotLights = lights.GetVisibleSpotLights(viewFrustum);

                this.DirLights = new[]
                {
                    dirLights.Length > 0 ? new BufferDirectionalLight(dirLights[0]) : new BufferDirectionalLight(),
                    dirLights.Length > 1 ? new BufferDirectionalLight(dirLights[1]) : new BufferDirectionalLight(),
                    dirLights.Length > 2 ? new BufferDirectionalLight(dirLights[2]) : new BufferDirectionalLight(),
                };
                this.PointLights = new[]
                {
                    pointLights.Length > 0 ? new BufferPointLight(pointLights[0]) : new BufferPointLight(),
                    pointLights.Length > 1 ? new BufferPointLight(pointLights[1]) : new BufferPointLight(),
                    pointLights.Length > 2 ? new BufferPointLight(pointLights[2]) : new BufferPointLight(),
                    pointLights.Length > 3 ? new BufferPointLight(pointLights[3]) : new BufferPointLight(),
                };
                this.SpotLights = new[]
                {
                    spotLights.Length > 0 ? new BufferSpotLight(spotLights[0]) : new BufferSpotLight(),
                    spotLights.Length > 1 ? new BufferSpotLight(spotLights[1]) : new BufferSpotLight(),
                    spotLights.Length > 2 ? new BufferSpotLight(spotLights[2]) : new BufferSpotLight(),
                    spotLights.Length > 3 ? new BufferSpotLight(spotLights[3]) : new BufferSpotLight(),
                };

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.FromLightViewProjection = fromLightViewProjection;
                this.ShadowMapStatic = shadowMapStatic;
                this.ShadowMapDynamic = shadowMapDynamic;
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

                this.DirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
                this.PointLights = new BufferPointLight[BufferPointLight.MAX];
                this.SpotLights = new BufferSpotLight[BufferSpotLight.MAX];

                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.FromLightViewProjection = Matrix.Identity;
                this.ShadowMapStatic = null;
                this.ShadowMapDynamic = null;
            }
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="material">Material</param>
        /// <param name="texture">Texture</param>
        /// <param name="normalMap">Normal map</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            Material material,
            ShaderResourceView texture,
            ShaderResourceView normalMap,
            int textureIndex)
        {
            this.Material = new BufferMaterials(material);
            this.Textures = texture;
            this.NormalMap = normalMap;
            this.TextureIndex = textureIndex;
        }
        /// <summary>
        /// Update per model skin data
        /// </summary>
        /// <param name="finalTransforms">Skinning final transforms</param>
        public void UpdatePerSkinning(Matrix[] finalTransforms)
        {
            this.BoneTransforms = finalTransforms;
        }
    }
}
