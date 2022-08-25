using Engine.Shaders.Properties;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectNull : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Null = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;

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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public EffectNull(Graphics graphics)
            : base(graphics, EffectsResources.ShaderNull, true)
        {
            Null = Effect.GetTechniqueByName("Null");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
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
            WorldViewProjection = world * viewProjection;
        }
    }
}
