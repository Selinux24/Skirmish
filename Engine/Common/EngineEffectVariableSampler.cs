
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Effect sampler variable
    /// </summary>
    public class EngineEffectVariableSampler
    {
        /// <summary>
        /// Effect sampler variable
        /// </summary>
        private readonly EffectSamplerVariable variable = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="variable">Internal effect sampler variable</param>
        public EngineEffectVariableSampler(EffectSamplerVariable variable)
        {
            this.variable = variable;
        }

        /// <summary>
        /// Gets a value of the specified type from the sampler variable
        /// </summary>
        /// <returns>Returns a value of the specified type from the sampler variable</returns>
        public SamplerState GetValue()
        {
            return this.variable.GetSampler();
        }
        /// <summary>
        /// Gets a value of the specified type from the sampler variable
        /// </summary>
        /// <param name="index">Sampler index</param>
        /// <returns>Returns a value of the specified type from the sampler variable</returns>
        public SamplerState GetValue(int index)
        {
            return this.variable.GetSampler(index);
        }

        /// <summary>
        /// Sets a value of the specified type from the variable
        /// </summary>
        /// <param name="index">Sampler index</param>
        /// <param name="samplerRef">Sampler reference</param>
        public void SetValue(int index, SamplerState samplerRef)
        {
            this.variable.SetSampler(index, samplerRef);
        }
    }
}
