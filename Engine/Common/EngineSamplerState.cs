using SharpDX;
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
        /// Internal sampler state
        /// </summary>
        private SamplerState state = null;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Creates a point sampler state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns the point sampler state</returns>
        public static EngineSamplerState Point(Graphics graphics, string name)
        {
            var desc = SamplerStateDescription.Default();
            desc.Filter = Filter.MinMagMipPoint;

            return graphics.CreateSamplerState($"{name}.{nameof(Point)}", desc);
        }
        /// <summary>
        /// Creates a linear sampler state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the linear sampler state</returns>
        public static EngineSamplerState Linear(Graphics graphics, string name)
        {
            var desc = SamplerStateDescription.Default();
            desc.Filter = Filter.MinMagMipLinear;
            desc.AddressU = TextureAddressMode.Wrap;
            desc.AddressV = TextureAddressMode.Wrap;

            return graphics.CreateSamplerState($"{name}.{nameof(Linear)}", desc);
        }
        /// <summary>
        /// Creates a anisotropic sampler state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="maxAnisotropic">Maximum anisotropic</param>
        /// <returns>Creates the anisotropic sampler state</returns>
        public static EngineSamplerState Anisotropic(Graphics graphics, string name, int maxAnisotropic)
        {
            var desc = SamplerStateDescription.Default();
            desc.Filter = Filter.Anisotropic;
            desc.MaximumAnisotropy = maxAnisotropic;
            desc.AddressU = TextureAddressMode.Wrap;
            desc.AddressV = TextureAddressMode.Wrap;

            return graphics.CreateSamplerState($"{name}.{nameof(Anisotropic)}_{maxAnisotropic}", desc);
        }
        /// <summary>
        /// Creates a ComparisonLessEqualBorder sampler state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns the ComparisonLessEqual sampler state</returns>
        public static EngineSamplerState ComparisonLessEqualBorder(Graphics graphics, string name)
        {
            var desc = SamplerStateDescription.Default();
            desc.Filter = Filter.ComparisonMinMagMipLinear;
            desc.AddressU = TextureAddressMode.Border;
            desc.AddressV = TextureAddressMode.Border;
            desc.BorderColor = Color4.White;
            desc.ComparisonFunction = Comparison.LessEqual;

            return graphics.CreateSamplerState($"{name}.{nameof(ComparisonLessEqualBorder)}", desc);
        }
        /// <summary>
        /// Creates a ComparisonLessEqualClamp sampler state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns the ComparisonLessEqual sampler state</returns>
        public static EngineSamplerState ComparisonLessEqualClamp(Graphics graphics, string name)
        {
            var desc = SamplerStateDescription.Default();
            desc.Filter = Filter.ComparisonMinMagMipLinear;
            desc.AddressU = TextureAddressMode.Clamp;
            desc.AddressV = TextureAddressMode.Clamp;
            desc.AddressW = TextureAddressMode.Clamp;
            desc.ComparisonFunction = Comparison.LessEqual;

            return graphics.CreateSamplerState($"{name}.{nameof(ComparisonLessEqualClamp)}", desc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="samplerState">Sampler state</param>
        internal EngineSamplerState(string name, SamplerState samplerState)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A sampler state name must be specified.");
            state = samplerState ?? throw new ArgumentNullException(nameof(samplerState), "A sampler state must be specified.");

            state.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineSamplerState()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                state?.Dispose();
                state = null;
            }
        }

        /// <summary>
        /// Gets the internal sampler state
        /// </summary>
        /// <returns>Returns the internal sampler state</returns>
        internal SamplerState GetSamplerState()
        {
            return state;
        }
    }
}
