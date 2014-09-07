using System;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using InputElement = SharpDX.Direct3D11.InputElement;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Common
{
    using Common.Properties;
    using Common.Utils;

    public class BasicEffect : EffectBase
    {
        private EffectMatrixVariable world = null;
        private EffectMatrixVariable worldInverse = null;
        private EffectMatrixVariable worldViewProjection = null;
        private EffectVariable material = null;
        private EffectVariable dirLights = null;
        private EffectVariable pointLight = null;
        private EffectVariable spotLight = null;
        private EffectVectorVariable eyePositionWorld = null;
        private EffectScalarVariable fogStart = null;
        private EffectScalarVariable fogRange = null;
        private EffectVectorVariable fogColor = null;
        private EffectShaderResourceVariable texture = null;

        public Matrix World
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
        public Matrix WorldInverse
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
        public Matrix WorldViewProjection
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
        public BufferMaterials Material
        {
            get
            {
                using (DataStream ds = this.material.GetRawValue(BufferMaterials.SizeInBytes))
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

                    this.material.SetRawValue(ds, BufferMaterials.SizeInBytes);
                }
            }
        }
        public DirectionalLight[] DirLights
        {
            get
            {
                using (DataStream ds = this.dirLights.GetRawValue(DirectionalLight.SizeInBytes * 3))
                {
                    ds.Position = 0;

                    return ds.ReadRange<DirectionalLight>(3);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<DirectionalLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.dirLights.SetRawValue(ds, DirectionalLight.SizeInBytes * 3);
                }
            }
        }
        public PointLight PointLight
        {
            get
            {
                using (DataStream ds = this.pointLight.GetRawValue(PointLight.SizeInBytes))
                {
                    ds.Position = 0;

                    return ds.Read<PointLight>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<PointLight>(new PointLight[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.pointLight.SetRawValue(ds, PointLight.SizeInBytes);
                }
            }
        }
        public SpotLight SpotLight
        {
            get
            {
                using (DataStream ds = this.spotLight.GetRawValue(SpotLight.SizeInBytes))
                {
                    ds.Position = 0;

                    return ds.Read<SpotLight>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<SpotLight>(new SpotLight[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.spotLight.SetRawValue(ds, SpotLight.SizeInBytes);
                }
            }
        }
        public Vector3 EyePositionWorld
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
        public float FogStart
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
        public float FogRange
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
        public Color4 FogColor
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
        public ShaderResourceView Texture
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

        public BasicEffect(Device device, VertexTypes vertexType)
            : base(vertexType)
        {
            InputElement[] inputElements;
            string selectedTechnique;

            if (this.VertexType == VertexTypes.PositionColor)
            {
                selectedTechnique = "PositionColor";
                inputElements = VertexPositionColor.GetInput();
            }
            else if (this.VertexType == VertexTypes.PositionNormalColor)
            {
                selectedTechnique = "PositionNormalColor";
                inputElements = VertexPositionNormalColor.GetInput();
            }
            else if (this.VertexType == VertexTypes.PositionTexture)
            {
                selectedTechnique = "PositionTexture";
                inputElements = VertexPositionTexture.GetInput();
            }
            else if (this.VertexType == VertexTypes.PositionNormalTexture)
            {
                selectedTechnique = "PositionNormalTexture";
                inputElements = VertexPositionNormalTexture.GetInput();
            }
            else
            {
                throw new Exception("Formato de vértice no válido");
            }

            this.effectInfo = ShaderUtils.LoadEffect(
                device,
                Resources.BasicShader,
                selectedTechnique,
                inputElements);

            this.SelectedTechnique = this.Effect.GetTechniqueByName(selectedTechnique);
            if (this.SelectedTechnique == null)
            {
                throw new Exception("Técnica no localizada según formato de vértice.");
            }

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldInverse = this.Effect.GetVariableByName("gWorldInverse").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.material = this.Effect.GetVariableByName("gMaterial");
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLight = this.Effect.GetVariableByName("gPointLight");
            this.spotLight = this.Effect.GetVariableByName("gSpotLight");
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.texture = this.Effect.GetVariableByName("gTexture").AsShaderResource();
        }
        public override void Dispose()
        {
            if (this.effectInfo != null)
            {
                this.effectInfo.Dispose();
                this.effectInfo = null;
            }
        }

        public override void UpdatePerFrame(
            BufferLights lBuffer)
        {
            this.DirLights = new DirectionalLight[]
            {
                lBuffer.DirectionalLight1,
                lBuffer.DirectionalLight2,
                lBuffer.DirectionalLight3,
            };
            this.PointLight = lBuffer.PointLight;
            this.SpotLight = lBuffer.SpotLight;
            this.EyePositionWorld = lBuffer.EyePositionWorld;
            this.FogStart = lBuffer.FogStart;
            this.FogRange = lBuffer.FogRange;
            this.FogColor = lBuffer.FogColor;
        }
        public override void UpdatePerObject(
            BufferMatrix mBuffer,
            ShaderResourceView texture)
        {
            this.World = mBuffer.World;
            this.WorldInverse = mBuffer.WorldInverse;
            this.WorldViewProjection = mBuffer.WorldViewProjection;
            this.Material = mBuffer.Material;
            this.Texture = this.Textured ? texture : null;
        }
    }
}
