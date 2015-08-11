using System;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectInstancingShadow : Drawer
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

        #endregion

        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// Bone transformation matrices effect variable
        /// </summary>
        private EffectMatrixVariable boneTransforms = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;

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
        /// Per frame buffer structure
        /// </summary>
        public EffectInstancingShadow.PerFrameBuffer FrameBuffer = new EffectInstancingShadow.PerFrameBuffer();
        /// <summary>
        /// Per skin buffer structure
        /// </summary>
        public EffectInstancingShadow.PerSkinningBuffer SkinningBuffer = new EffectInstancingShadow.PerSkinningBuffer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectInstancingShadow(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ShadowMapPositionColor = this.Effect.GetTechniqueByName("ShadowMapPositionColorI");
            this.ShadowMapPositionColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinnedI");
            this.ShadowMapPositionNormalColor = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorI");
            this.ShadowMapPositionNormalColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinnedI");
            this.ShadowMapPositionTexture = this.Effect.GetTechniqueByName("ShadowMapPositionTextureI");
            this.ShadowMapPositionTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinnedI");
            this.ShadowMapPositionNormalTexture = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureI");
            this.ShadowMapPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinnedI");
            this.ShadowMapPositionNormalTextureTangent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentI");
            this.ShadowMapPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinnedI");

            this.AddInputLayout(this.ShadowMapPositionColor, VertexPositionColor.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionColorSkinned, VertexSkinnedPositionColor.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionNormalColor, VertexPositionNormalColor.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionNormalColorSkinned, VertexSkinnedPositionNormalColor.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionTexture, VertexPositionTexture.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionTextureSkinned, VertexSkinnedPositionTexture.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionNormalTexture, VertexPositionNormalTexture.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput().Join(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.ShadowMapPositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput().Join(VertexInstancingData.GetInput()));

            this.boneTransforms = this.Effect.GetVariableByName("gBoneTransforms").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
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
                    return this.ShadowMapPositionColor;
                }
                else if (vertexType == VertexTypes.PositionColorSkinned)
                {
                    return this.ShadowMapPositionNormalColorSkinned;
                }
                else if (vertexType == VertexTypes.PositionNormalColor)
                {
                    return this.ShadowMapPositionNormalColor;
                }
                else if (vertexType == VertexTypes.PositionNormalColorSkinned)
                {
                    return this.ShadowMapPositionNormalColorSkinned;
                }
                else if (vertexType == VertexTypes.PositionTexture)
                {
                    return this.ShadowMapPositionTexture;
                }
                else if (vertexType == VertexTypes.PositionTextureSkinned)
                {
                    return this.ShadowMapPositionTextureSkinned;
                }
                else if (vertexType == VertexTypes.PositionNormalTexture)
                {
                    return this.ShadowMapPositionNormalTexture;
                }
                else if (vertexType == VertexTypes.PositionNormalTextureSkinned)
                {
                    return this.ShadowMapPositionNormalTextureSkinned;
                }
                else if (vertexType == VertexTypes.PositionNormalTextureTangent)
                {
                    return this.ShadowMapPositionNormalTextureTangent;
                }
                else if (vertexType == VertexTypes.PositionNormalTextureTangentSkinned)
                {
                    return this.ShadowMapPositionNormalTextureTangentSkinned;
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
        /// Update per model skin data
        /// </summary>
        public void UpdatePerSkinning()
        {
            if (this.SkinningBuffer.FinalTransforms != null)
            {
                this.BoneTransforms = this.SkinningBuffer.FinalTransforms;
            }
        }
    }
}
