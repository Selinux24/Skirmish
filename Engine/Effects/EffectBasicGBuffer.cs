using System;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectBasicGBuffer : Drawer
    {
        /// <summary>
        /// Maximum number of bones in a skeleton
        /// </summary>
        public const int MaxBoneTransforms = 96;

        #region Buffers

        /// <summary>
        /// Per frame update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public Matrix World;
            public Matrix WorldInverse;
            public Matrix WorldViewProjection;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerFrameBuffer));
                }
            }
        }
        /// <summary>
        /// Per model object update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerObjectBuffer
        {
            public BufferMaterials Material;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerObjectBuffer));
                }
            }
        }
        /// <summary>
        /// Per model skin update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerSkinningBuffer
        {
            public Matrix[] FinalTransforms;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(Matrix)) * MaxBoneTransforms;
                }
            }
        }
        /// <summary>
        /// Per model instance update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerInstanceBuffer
        {
            public float TextureIndex;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerInstanceBuffer));
                }
            }
        }

        #endregion

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
        /// Position texture drawing technique
        /// </summary>
        public readonly EffectTechnique PositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureTangentSkinned = null;

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
        /// Material effect variable
        /// </summary>
        private EffectVariable material = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable textures = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EffectScalarVariable textureIndex = null;
        /// <summary>
        /// Bone transformation matrices effect variable
        /// </summary>
        private EffectMatrixVariable boneTransforms = null;

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

                this.boneTransforms.SetMatrix(value);
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
        /// Per frame buffer structure
        /// </summary>
        public EffectBasicGBuffer.PerFrameBuffer FrameBuffer = new EffectBasicGBuffer.PerFrameBuffer();
        /// <summary>
        /// Per model object buffer structure
        /// </summary>
        public EffectBasicGBuffer.PerObjectBuffer ObjectBuffer = new EffectBasicGBuffer.PerObjectBuffer();
        /// <summary>
        /// Per skin buffer structure
        /// </summary>
        public EffectBasicGBuffer.PerSkinningBuffer SkinningBuffer = new EffectBasicGBuffer.PerSkinningBuffer();
        /// <summary>
        /// Per model instance buffer structure
        /// </summary>
        public EffectBasicGBuffer.PerInstanceBuffer InstanceBuffer = new EffectBasicGBuffer.PerInstanceBuffer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectBasicGBuffer(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.PositionColor = this.Effect.GetTechniqueByName("PositionColor");
            this.PositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinned");
            this.PositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColor");
            this.PositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinned");
            this.PositionTexture = this.Effect.GetTechniqueByName("PositionTexture");
            this.PositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinned");
            this.PositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTexture");
            this.PositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinned");
            this.PositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangent");
            this.PositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinned");

            this.AddInputLayout(this.PositionColor, VertexPositionColor.GetInput());
            this.AddInputLayout(this.PositionColorSkinned, VertexSkinnedPositionColor.GetInput());
            this.AddInputLayout(this.PositionNormalColor, VertexPositionNormalColor.GetInput());
            this.AddInputLayout(this.PositionNormalColorSkinned, VertexSkinnedPositionNormalColor.GetInput());
            this.AddInputLayout(this.PositionTexture, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureSkinned, VertexSkinnedPositionTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTexture, VertexPositionNormalTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput());
            this.AddInputLayout(this.PositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldInverse = this.Effect.GetVariableByName("gWorldInverse").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.material = this.Effect.GetVariableByName("gMaterial");
            this.textures = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();
            this.boneTransforms = this.Effect.GetVariableByName("gBoneTransforms").AsMatrix();
            this.textureIndex = this.Effect.GetVariableByName("gTextureIndex").AsScalar();
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
                else if (vertexType == VertexTypes.PositionColorSkinned)
                {
                    return this.PositionNormalColorSkinned;
                }
                else if (vertexType == VertexTypes.PositionNormalColor)
                {
                    return this.PositionNormalColor;
                }
                else if (vertexType == VertexTypes.PositionNormalColorSkinned)
                {
                    return this.PositionNormalColorSkinned;
                }
                else if (vertexType == VertexTypes.PositionTexture)
                {
                    return this.PositionTexture;
                }
                else if (vertexType == VertexTypes.PositionTextureSkinned)
                {
                    return this.PositionTextureSkinned;
                }
                else if (vertexType == VertexTypes.PositionNormalTexture)
                {
                    return this.PositionNormalTexture;
                }
                else if (vertexType == VertexTypes.PositionNormalTextureSkinned)
                {
                    return this.PositionNormalTextureSkinned;
                }
                else if (vertexType == VertexTypes.PositionNormalTextureTangent)
                {
                    return this.PositionNormalTextureTangent;
                }
                else if (vertexType == VertexTypes.PositionNormalTextureTangentSkinned)
                {
                    return this.PositionNormalTextureTangentSkinned;
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
            this.World = this.FrameBuffer.World;
            this.WorldInverse = this.FrameBuffer.WorldInverse;
            this.WorldViewProjection = this.FrameBuffer.WorldViewProjection;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(ShaderResourceView texture)
        {
            this.Material = this.ObjectBuffer.Material;
            this.Textures = texture;
        }
        /// <summary>
        /// Update per model skin data
        /// </summary>
        public void UpdatePerSkinning()
        {
            if (this.SkinningBuffer.FinalTransforms != null)
            {
                this.BoneTransforms = this.SkinningBuffer.FinalTransforms;
            }
        }
        /// <summary>
        /// Update per model instance data
        /// </summary>
        public void UpdatePerInstance()
        {
            this.TextureIndex = (int)this.InstanceBuffer.TextureIndex;
        }
    }
}
