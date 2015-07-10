using System;
using System.Runtime.InteropServices;
using SharpDX;
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
    public class EffectParticles : Drawer
    {
        #region Buffers

        /// <summary>
        /// Per frame update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public float MaxAge;
            public float EmitAge;
            public float GameTime;
            public float TimeStep;
            public Vector3 AccelerationWorld;
            public Vector3 EyePositionWorld;
            public Matrix World;
            public Matrix WorldViewProjection;
            public uint TextureCount;

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
        /// Fire stream out technique
        /// </summary>
        public readonly EffectTechnique FireStreamOut = null;
        /// <summary>
        /// Rain stream out technique
        /// </summary>
        public readonly EffectTechnique RainStreamOut = null;
        /// <summary>
        /// Smoke stream out technique
        /// </summary>
        public readonly EffectTechnique SmokeStreamOut = null;
        /// <summary>
        /// Solid drawing technique
        /// </summary>
        public readonly EffectTechnique SolidDraw = null;
        /// <summary>
        /// Line drawing technique
        /// </summary>
        public readonly EffectTechnique LineDraw = null;

        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
        /// <summary>
        /// Game time effect variable
        /// </summary>
        private EffectScalarVariable totalTime = null;
        /// <summary>
        /// Time step effect variable
        /// </summary>
        private EffectScalarVariable elapsedTime = null;
        /// <summary>
        /// World effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Acceleration effect variable
        /// </summary>
        private EffectVectorVariable accelerationWorld = null;
        /// <summary>
        /// Emit age effect variable
        /// </summary>
        private EffectScalarVariable emitterAge = null;
        /// <summary>
        /// Maximum age effect variable
        /// </summary>
        private EffectScalarVariable maximumAge = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private EffectScalarVariable textureCount = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private EffectShaderResourceVariable textureArray = null;
        /// <summary>
        /// Random texture effect variable
        /// </summary>
        private EffectShaderResourceVariable textureRandom = null;

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
        /// Time step
        /// </summary>
        protected float ElapsedTime
        {
            get
            {
                return this.elapsedTime.GetFloat();
            }
            set
            {
                this.elapsedTime.Set(value);
            }
        }
        /// <summary>
        /// Acceleration
        /// </summary>
        protected Vector3 AccelerationWorld
        {
            get
            {
                Vector4 v = this.accelerationWorld.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.accelerationWorld.Set(v4);
            }
        }
        /// <summary>
        /// Emit age
        /// </summary>
        protected float EmitterAge
        {
            get
            {
                return this.emitterAge.GetFloat();
            }
            set
            {
                this.emitterAge.Set(value);
            }
        }
        /// <summary>
        /// Maximum age
        /// </summary>
        protected float MaximumAge
        {
            get
            {
                return this.maximumAge.GetFloat();
            }
            set
            {
                this.maximumAge.Set(value);
            }
        }
        /// <summary>
        /// World
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
        /// <summary>
        /// Random texture
        /// </summary>
        protected ShaderResourceView TextureRandom
        {
            get
            {
                return this.textureRandom.GetResource();
            }
            set
            {
                this.textureRandom.SetResource(value);
            }
        }

        /// <summary>
        /// Per frame buffer structure
        /// </summary>
        public EffectParticles.PerFrameBuffer FrameBuffer = new EffectParticles.PerFrameBuffer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectParticles(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.FireStreamOut = this.Effect.GetTechniqueByName("FireStreamOut");
            this.RainStreamOut = this.Effect.GetTechniqueByName("RainStreamOut");
            this.SmokeStreamOut = this.Effect.GetTechniqueByName("SmokeStreamOut");
            this.SolidDraw = this.Effect.GetTechniqueByName("SolidDraw");
            this.LineDraw = this.Effect.GetTechniqueByName("LineDraw");

            this.AddInputLayout(this.FireStreamOut, VertexParticle.GetInput());
            this.AddInputLayout(this.RainStreamOut, VertexParticle.GetInput());
            this.AddInputLayout(this.SmokeStreamOut, VertexParticle.GetInput());
            this.AddInputLayout(this.SolidDraw, VertexParticle.GetInput());
            this.AddInputLayout(this.LineDraw, VertexParticle.GetInput());

            this.emitterAge = this.Effect.GetVariableByName("gEmitterAge").AsScalar();
            this.maximumAge = this.Effect.GetVariableByName("gMaximumAge").AsScalar();
            this.totalTime = this.Effect.GetVariableByName("gTotalTime").AsScalar();
            this.elapsedTime = this.Effect.GetVariableByName("gElapsedTime").AsScalar();
            this.accelerationWorld = this.Effect.GetVariableByName("gAccelerationWorld").AsVector();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.textureCount = this.Effect.GetVariableByName("gTextureCount").AsScalar();

            this.textureArray = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();
            this.textureRandom = this.Effect.GetVariableByName("gTextureRandom").AsShaderResource();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="stage">Stage</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, DrawingStages stage)
        {
            throw new Exception(string.Format("Bad stage for effect. Use particle class: {0}", stage));
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="stage">Stage</param>
        /// <param name="particleClass">Particle class</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public EffectTechnique GetTechnique(VertexTypes vertexType, DrawingStages stage, ParticleClasses particleClass)
        {
            if (stage == DrawingStages.StreamOut)
            {
                if (vertexType == VertexTypes.Particle)
                {
                    if (particleClass == ParticleClasses.Fire) return this.FireStreamOut;
                    else if (particleClass == ParticleClasses.Smoke) return this.SmokeStreamOut;
                    else if (particleClass == ParticleClasses.Rain) return this.RainStreamOut;
                    else throw new Exception(string.Format("Bad particle class: {0}", particleClass));
                }
                else
                {
                    throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                }
            }
            else if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Particle)
                {
                    if (particleClass == ParticleClasses.Fire) return this.SolidDraw;
                    else if (particleClass == ParticleClasses.Smoke) return this.SolidDraw;
                    else if (particleClass == ParticleClasses.Rain) return this.LineDraw;
                    else throw new Exception(string.Format("Bad particle class: {0}", particleClass));
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
        /// <param name="randomTexture">Texture with random numbers</param>
        public void UpdatePerFrame(ShaderResourceView textures, ShaderResourceView randomTexture)
        {
            this.EmitterAge = this.FrameBuffer.EmitAge;
            this.MaximumAge = this.FrameBuffer.MaxAge;
            this.TotalTime = this.FrameBuffer.GameTime;
            this.ElapsedTime = this.FrameBuffer.TimeStep;
            this.AccelerationWorld = this.FrameBuffer.AccelerationWorld;
            this.EyePositionWorld = this.FrameBuffer.EyePositionWorld;
            this.World = this.FrameBuffer.World;
            this.WorldViewProjection = this.FrameBuffer.WorldViewProjection;
            this.TextureCount = this.FrameBuffer.TextureCount;

            this.TextureArray = textures;
            this.TextureRandom = randomTexture;
        }
    }
}
