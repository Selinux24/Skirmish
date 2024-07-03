
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Effect sampler variable
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="variable">Internal effect sampler variable</param>
    public class EngineEffectVariableSampler(EffectSamplerVariable variable)
    {
        /// <summary>
        /// Effect sampler variable
        /// </summary>
        private readonly EffectSamplerVariable variable = variable;

        /// <summary>
        /// Gets a value of the specified type from the sampler variable
        /// </summary>
        /// <returns>Returns a value of the specified type from the sampler variable</returns>
        public SamplerState GetValue()
        {
            return variable.GetSampler();
        }
        /// <summary>
        /// Gets a value of the specified type from the sampler variable
        /// </summary>
        /// <param name="index">Sampler index</param>
        /// <returns>Returns a value of the specified type from the sampler variable</returns>
        public SamplerState GetValue(int index)
        {
            return variable.GetSampler(index);
        }

        /// <summary>
        /// Sets a value of the specified type from the variable
        /// </summary>
        /// <param name="index">Sampler index</param>
        /// <param name="samplerRef">Sampler reference</param>
        public void SetValue(int index, SamplerState samplerRef)
        {
            variable.SetSampler(index, samplerRef);
        }
    }
}
