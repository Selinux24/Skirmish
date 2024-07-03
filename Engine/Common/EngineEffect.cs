using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Effect
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="effect">Effect</param>
    public class EngineEffect(Effect effect) : IDisposable
    {
        /// <summary>
        /// Effect
        /// </summary>
        private Effect effect = effect;

        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineEffect()
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
                effect?.Dispose();
                effect = null;
            }
        }

        /// <summary>
        /// Optimizes the effect
        /// </summary>
        public void Optimize()
        {
            if (!effect.IsOptimized)
            {
                effect.Optimize();
            }
        }

        /// <summary>
        /// Gets a technique by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the technique</returns>
        public EngineEffectTechnique GetTechniqueByName(string name)
        {
            return new EngineEffectTechnique(effect.GetTechniqueByName(name));
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariable GetVariable(string name)
        {
            return new EngineEffectVariable(effect.GetVariableByName(name));
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableTexture GetVariableTexture(string name)
        {
            return new EngineEffectVariableTexture(effect.GetVariableByName(name).AsShaderResource());
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableScalar GetVariableScalar(string name)
        {
            return new EngineEffectVariableScalar(effect.GetVariableByName(name).AsScalar());
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableVector GetVariableVector(string name)
        {
            return new EngineEffectVariableVector(effect.GetVariableByName(name).AsVector());
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableMatrix GetVariableMatrix(string name)
        {
            return new EngineEffectVariableMatrix(effect.GetVariableByName(name).AsMatrix());
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableSampler GetVariableSampler(string name)
        {
            return new EngineEffectVariableSampler(effect.GetVariableByName(name).AsSampler());
        }
    }
}
