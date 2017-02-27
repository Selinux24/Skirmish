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
    public class EffectDeferredTerrain : Drawer
    {
        /// <summary>
        /// Deferred with alpha map drawing technique
        /// </summary>
        public readonly EffectTechnique TerrainAlphaMapDeferred = null;
        /// <summary>
        /// Deferred with slopes drawing technique
        /// </summary>
        public readonly EffectTechnique TerrainSlopesDeferred = null;
        /// <summary>
        /// Deferred full drawing technique
        /// </summary>
        public readonly EffectTechnique TerrainFullDeferred = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Texture resolution effect variable
        /// </summary>
        private EffectScalarVariable textureResolution = null;
        /// <summary>
        /// Low resolution textures effect variable
        /// </summary>
        private EffectShaderResourceVariable diffuseMapLR = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private EffectShaderResourceVariable diffuseMapHR = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EffectShaderResourceVariable normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private EffectShaderResourceVariable specularMap = null;
        /// <summary>
        /// Color texture array effect variable
        /// </summary>
        private EffectShaderResourceVariable colorTextures = null;
        /// <summary>
        /// Alpha map effect variable
        /// </summary>
        private EffectShaderResourceVariable alphaMap = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private EffectVectorVariable parameters = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private EffectScalarVariable materialIndex = null;

        /// <summary>
        /// Current low resolution diffuse map
        /// </summary>
        private ShaderResourceView currentDiffuseMapLR = null;
        /// <summary>
        /// Current hihg resolution diffuse map
        /// </summary>
        private ShaderResourceView currentDiffuseMapHR = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private ShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current specular map
        /// </summary>
        private ShaderResourceView currentSpecularMap = null;
        /// <summary>
        /// Current color texture array
        /// </summary>
        private ShaderResourceView currentColorTextures = null;
        /// <summary>
        /// Current alpha map
        /// </summary>
        private ShaderResourceView currentAlphaMap = null;

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
        /// Texture resolution
        /// </summary>
        protected float TextureResolution
        {
            get
            {
                return this.textureResolution.GetFloat();
            }
            set
            {
                this.textureResolution.Set(value);
            }
        }
        /// <summary>
        /// Low resolution textures
        /// </summary>
        protected ShaderResourceView DiffuseMapLR
        {
            get
            {
                return this.diffuseMapLR.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapLR != value)
                {
                    this.diffuseMapLR.SetResource(value);

                    this.currentDiffuseMapLR = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// High resolution textures
        /// </summary>
        protected ShaderResourceView DiffuseMapHR
        {
            get
            {
                return this.diffuseMapHR.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapHR != value)
                {
                    this.diffuseMapHR.SetResource(value);

                    this.currentDiffuseMapHR = value;

                    Counters.TextureUpdates++;
                }
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
                if (this.currentNormalMap != value)
                {
                    this.normalMap.SetResource(value);

                    this.currentNormalMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Scpecular map
        /// </summary>
        protected ShaderResourceView SpecularMap
        {
            get
            {
                return this.specularMap.GetResource();
            }
            set
            {
                if (this.currentSpecularMap != value)
                {
                    this.specularMap.SetResource(value);

                    this.currentSpecularMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Color textures for alpha map
        /// </summary>
        protected ShaderResourceView ColorTextures
        {
            get
            {
                return this.colorTextures.GetResource();
            }
            set
            {
                if (this.currentColorTextures != value)
                {
                    this.colorTextures.SetResource(value);

                    this.currentColorTextures = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Alpha map
        /// </summary>
        protected ShaderResourceView AlphaMap
        {
            get
            {
                return this.alphaMap.GetResource();
            }
            set
            {
                if (this.currentAlphaMap != value)
                {
                    this.alphaMap.SetResource(value);

                    this.currentAlphaMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Slope ranges
        /// </summary>
        protected Vector4 Parameters
        {
            get
            {
                return this.parameters.GetFloatVector();
            }
            set
            {
                this.parameters.Set(value);
            }
        }
        /// <summary>
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return (uint)this.materialIndex.GetFloat();
            }
            set
            {
                this.materialIndex.Set((float)value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDeferredTerrain(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.TerrainAlphaMapDeferred = this.Effect.GetTechniqueByName("TerrainAlphaMapDeferred");
            this.TerrainSlopesDeferred = this.Effect.GetTechniqueByName("TerrainSlopesDeferred");
            this.TerrainFullDeferred = this.Effect.GetTechniqueByName("TerrainFullDeferred");

            this.world = this.Effect.GetVariableByName("gVSWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gVSWorldViewProjection").AsMatrix();
            this.textureResolution = this.Effect.GetVariableByName("gVSTextureResolution").AsScalar();

            this.diffuseMapLR = this.Effect.GetVariableByName("gPSDiffuseMapLRArray").AsShaderResource();
            this.diffuseMapHR = this.Effect.GetVariableByName("gPSDiffuseMapHRArray").AsShaderResource();
            this.normalMap = this.Effect.GetVariableByName("gPSNormalMapArray").AsShaderResource();
            this.specularMap = this.Effect.GetVariableByName("gPSSpecularMapArray").AsShaderResource();
            this.colorTextures = this.Effect.GetVariableByName("gPSColorTextureArray").AsShaderResource();
            this.alphaMap = this.Effect.GetVariableByName("gPSAlphaTexture").AsShaderResource();
            this.parameters = this.Effect.GetVariableByName("gPSParams").AsVector();
            this.materialIndex = this.Effect.GetVariableByName("gPSMaterialIndex").AsScalar();
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
            EffectTechnique technique = null;

            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Terrain)
                {
                    if (mode == DrawerModesEnum.Deferred) technique = this.TerrainFullDeferred;
                }
            }

            if (technique == null)
            {
                throw new Exception(string.Format("Bad vertex type for effect, stage and mode: {0} - {1} - {2}", vertexType, stage, mode));
            }

            return technique;
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="textureResolution">Texture resolution</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float textureResolution)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.TextureResolution = textureResolution;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="materialIndex">Material index</param>
        /// <param name="normalMap">Normal map</param>
        /// <param name="specularMap">Specular map</param>
        /// <param name="useAlphaMap">Use alpha mapping</param>
        /// <param name="alphaMap">Alpha map</param>
        /// <param name="colorTextures">Color textures</param>
        /// <param name="useSlopes">Use slope texturing</param>
        /// <param name="diffuseMapLR">Low resolution textures</param>
        /// <param name="diffuseMapHR">High resolution textures</param>
        /// <param name="slopeRanges">Slope ranges</param>
        /// <param name="proportion">Lerping proportion</param>
        public void UpdatePerObject(
            uint materialIndex,
            ShaderResourceView normalMap,
            ShaderResourceView specularMap,
            bool useAlphaMap,
            ShaderResourceView alphaMap,
            ShaderResourceView colorTextures,
            bool useSlopes,
            Vector2 slopeRanges,
            ShaderResourceView diffuseMapLR,
            ShaderResourceView diffuseMapHR,
            float proportion)
        {
            this.MaterialIndex = materialIndex;
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;

            this.AlphaMap = alphaMap;
            this.ColorTextures = colorTextures;

            this.DiffuseMapLR = diffuseMapLR;
            this.DiffuseMapHR = diffuseMapHR;

            this.Parameters = new Vector4(0, proportion, slopeRanges.X, slopeRanges.Y);
        }
    }
}
