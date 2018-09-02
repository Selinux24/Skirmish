using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Effect
    /// </summary>
    public class EngineEffect : IDisposable
    {
        /// <summary>
        /// Effect
        /// </summary>
        private Effect effect = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="effect">Effect</param>
        public EngineEffect(Effect effect)
        {
            this.effect = effect;
        }
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
                if (this.effect != null)
                {
                    this.effect.Dispose();
                    this.effect = null;
                }
            }
        }

        /// <summary>
        /// Optimizes the effect
        /// </summary>
        public void Optimize()
        {
            if (!this.effect.IsOptimized)
            {
                this.effect.Optimize();
            }
        }

        /// <summary>
        /// Gets a technique by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the technique</returns>
        public EngineEffectTechnique GetTechniqueByName(string name)
        {
            return new EngineEffectTechnique(this.effect.GetTechniqueByName(name));
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariable GetVariable(string name)
        {
            return new EngineEffectVariable(this.effect.GetVariableByName(name));
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableTexture GetVariableTexture(string name)
        {
            return new EngineEffectVariableTexture(this.effect.GetVariableByName(name).AsShaderResource());
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableScalar GetVariableScalar(string name)
        {
            return new EngineEffectVariableScalar(this.effect.GetVariableByName(name).AsScalar());
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableVector GetVariableVector(string name)
        {
            return new EngineEffectVariableVector(this.effect.GetVariableByName(name).AsVector());
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableMatrix GetVariableMatrix(string name)
        {
            return new EngineEffectVariableMatrix(this.effect.GetVariableByName(name).AsMatrix());
        }
        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the variable</returns>
        public EngineEffectVariableSampler GetVariableSampler(string name)
        {
            return new EngineEffectVariableSampler(this.effect.GetVariableByName(name).AsSampler());
        }
    }
}
