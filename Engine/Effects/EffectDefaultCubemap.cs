using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Cube map effect
    /// </summary>
    public class EffectDefaultCubemap : Drawer
    {
        /// <summary>
        /// Cubemap drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ForwardCubemap = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture cubeTextureVar = null;

        /// <summary>
        /// Current cube texture
        /// </summary>
        private EngineShaderResourceView currentCubeTexture = null;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return worldViewProjectionVar.GetMatrix();
            }
            set
            {
                worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView CubeTexture
        {
            get
            {
                return cubeTextureVar.GetResource();
            }
            set
            {
                if (currentCubeTexture != value)
                {
                    cubeTextureVar.SetResource(value);

                    currentCubeTexture = value;

                    Counters.TextureUpdates++;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultCubemap(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            ForwardCubemap = Effect.GetTechniqueByName("ForwardCubemap");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            cubeTextureVar = Effect.GetVariableTexture("gCubemap");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            WorldViewProjection = world * viewProjection;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(
            EngineShaderResourceView cubeTexture)
        {
            CubeTexture = cubeTexture;
        }
    }
}
