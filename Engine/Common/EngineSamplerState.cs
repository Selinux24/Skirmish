using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Engine sampler state
    /// </summary>
    public class EngineSamplerState : IDisposable
    {
        /// <summary>
        /// Internal blend state
        /// </summary>
        private SamplerState samplerState = null;

        /// <summary>
        /// Creates a point sampler state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns the point sampler state</returns>
        public static EngineSamplerState Point(Graphics graphics)
        {
            var desc = SamplerStateDescription.Default();
            desc.Filter = Filter.MinMagMipPoint;

            return graphics.CreateSamplerState(desc);
        }
        /// <summary>
        /// Creates a linear sampler state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the linear sampler state</returns>
        public static EngineSamplerState Linear(Graphics graphics)
        {
            var desc = SamplerStateDescription.Default();
            desc.Filter = Filter.MinMagMipLinear;
            desc.AddressU = TextureAddressMode.Wrap;
            desc.AddressV = TextureAddressMode.Wrap;

            return graphics.CreateSamplerState(desc);
        }
        /// <summary>
        /// Creates a anisotropic sampler state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="maxAnisotropic">Maximum anisotropic</param>
        /// <returns>Creates the anisotropic sampler state</returns>
        public static EngineSamplerState Anisotropic(Graphics graphics, int maxAnisotropic)
        {
            var desc = SamplerStateDescription.Default();
            desc.Filter = Filter.Anisotropic;
            desc.MaximumAnisotropy = maxAnisotropic;
            desc.AddressU = TextureAddressMode.Wrap;
            desc.AddressV = TextureAddressMode.Wrap;

            return graphics.CreateSamplerState(desc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplerState">Sampler state</param>
        internal EngineSamplerState(SamplerState samplerState)
        {
            this.samplerState = samplerState;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (this.samplerState != null)
            {
                this.samplerState.Dispose();
                this.samplerState = null;
            }
        }

        /// <summary>
        /// Gets the internal sampler state
        /// </summary>
        /// <returns>Returns the internal sampler state</returns>
        internal SamplerState GetSamplerState()
        {
            return this.samplerState;
        }
    }
}
