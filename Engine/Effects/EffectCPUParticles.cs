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
    /// Particles effect
    /// </summary>
    public class EffectCPUParticles : Drawer
    {
        /// <summary>
        /// Forward drawing technique
        /// </summary>
        public readonly EffectTechnique ForwardDraw = null;

        /// <summary>
        /// World effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
        /// <summary>
        /// Game time effect variable
        /// </summary>
        private EffectScalarVariable totalTime = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private EffectScalarVariable textureCount = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private EffectShaderResourceVariable textureArray = null;



        private EffectScalarVariable viewportHeight = null;
        private EffectScalarVariable maxDuration = null;
        private EffectScalarVariable maxDurationRandomness = null;
        private EffectScalarVariable endVelocity = null;
        private EffectVectorVariable gravity = null;
        private EffectVectorVariable startSize = null;
        private EffectVectorVariable endSize = null;
        private EffectVectorVariable minColor = null;
        private EffectVectorVariable maxColor = null;
        private EffectVectorVariable rotateSpeed = null;


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
        /// Game time
        /// </summary>
        protected float TotalTime
        {
            get
            {
                return this.totalTime.GetFloat();
            }
            set
            {
                this.totalTime.Set(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return (uint)this.textureCount.GetInt();
            }
            set
            {
                this.textureCount.Set(value);
            }
        }
        /// <summary>
        /// Textures
        /// </summary>
        protected ShaderResourceView TextureArray
        {
            get
            {
                return this.textureArray.GetResource();
            }
            set
            {
                this.textureArray.SetResource(value);
            }
        }


        protected float ViewportHeight
        {
            get
            {
                return this.viewportHeight.GetFloat();
            }
            set
            {
                this.viewportHeight.Set(value);
            }
        }
        protected float MaxDuration
        {
            get
            {
                return this.maxDuration.GetFloat();
            }
            set
            {
                this.maxDuration.Set(value);
            }
        }
        protected float MaxDurationRandomness
        {
            get
            {
                return this.maxDurationRandomness.GetFloat();
            }
            set
            {
                this.maxDurationRandomness.Set(value);
            }
        }
        protected float EndVelocity
        {
            get
            {
                return this.endVelocity.GetFloat();
            }
            set
            {
                this.endVelocity.Set(value);
            }
        }
        protected Vector3 Gravity
        {
            get
            {
                Vector4 v = this.gravity.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.gravity.Set(v4);
            }
        }
        protected Vector2 StartSize
        {
            get
            {
                Vector4 v = this.startSize.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 1f, 1f);

                this.startSize.Set(v4);
            }
        }
        protected Vector2 EndSize
        {
            get
            {
                Vector4 v = this.endSize.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 1f, 1f);

                this.endSize.Set(v4);
            }
        }
        protected Color4 MinColor
        {
            get
            {
                return new Color4(this.minColor.GetFloatVector());
            }
            set
            {
                this.minColor.Set(value);
            }
        }
        protected Color4 MaxColor
        {
            get
            {
                return new Color4(this.maxColor.GetFloatVector());
            }
            set
            {
                this.maxColor.Set(value);
            }
        }
        protected Vector2 RotateSpeed
        {
            get
            {
                Vector4 v = this.rotateSpeed.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 1f, 1f);

                this.rotateSpeed.Set(v4);
            }
        }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectCPUParticles(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ForwardDraw = this.Effect.GetTechniqueByName("ForwardParticle");

            this.AddInputLayout(this.ForwardDraw, VertexCPUParticle.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.totalTime = this.Effect.GetVariableByName("gTotalTime").AsScalar();
            this.textureCount = this.Effect.GetVariableByName("gTextureCount").AsScalar();
            this.textureArray = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();

            this.viewportHeight = this.Effect.GetVariableByName("gViewportHeight").AsScalar();
            this.maxDuration = this.Effect.GetVariableByName("gMaxDuration").AsScalar();
            this.maxDurationRandomness = this.Effect.GetVariableByName("gMaxDurationRandomness").AsScalar();
            this.endVelocity = this.Effect.GetVariableByName("gEndVelocity").AsScalar();
            this.gravity = this.Effect.GetVariableByName("gGravity").AsVector();
            this.startSize = this.Effect.GetVariableByName("gStartSize").AsVector();
            this.endSize = this.Effect.GetVariableByName("gEndSize").AsVector();
            this.minColor = this.Effect.GetVariableByName("gMinColor").AsVector();
            this.maxColor = this.Effect.GetVariableByName("gMaxColor").AsVector();
            this.rotateSpeed = this.Effect.GetVariableByName("gRotateSpeed").AsVector();
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
                if (vertexType == VertexTypes.Particle)
                {
                    switch (mode)
                    {
                        case DrawerModesEnum.Forward:
                            return this.ForwardDraw;
                        default:
                            throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                    }
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
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="totalTime">Total time</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float vpHeight,
            Vector3 eyePositionWorld,
            float totalTime,
            float maxDuration,
            float maxDurationRandomness,
            float endVelocity,
            Vector3 gravity,
            Vector2 startSize,
            Vector2 endSize,
            Color4 minColor,
            Color4 maxColor,
            Vector2 rotateSpeed,
            uint textureCount,
            ShaderResourceView textures)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.ViewportHeight = vpHeight;
            this.EyePositionWorld = eyePositionWorld;
            this.TotalTime = totalTime;
            this.MaxDuration = maxDuration;
            this.MaxDurationRandomness = maxDurationRandomness;
            this.EndVelocity = endVelocity;
            this.Gravity = gravity;
            this.StartSize = startSize;
            this.EndSize = endSize;
            this.MinColor = minColor;
            this.MaxColor = maxColor;
            this.RotateSpeed = rotateSpeed;
            this.TextureCount = textureCount;
            this.TextureArray = textures;
        }
    }
}
