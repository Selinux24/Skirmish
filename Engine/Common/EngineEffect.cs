using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    public class EngineEffect : IDisposable
    {
        private Effect effect = null;

        internal EngineEffect(Effect effect)
        {
            this.effect = effect;
        }

        public EngineEffectTechnique GetTechniqueByName(string name)
        {
            return new EngineEffectTechnique(this.effect.GetTechniqueByName(name));
        }

        internal EngineEffectVariable GetVariable(string name)
        {
            return new EngineEffectVariable(this.effect.GetVariableByName(name));
        }
        internal EngineEffectVariableTexture GetVariableTexture(string name)
        {
            return new EngineEffectVariableTexture(this.effect.GetVariableByName(name).AsShaderResource());
        }
        internal EngineEffectVariableScalar GetVariableScalar(string name)
        {
            return new EngineEffectVariableScalar(this.effect.GetVariableByName(name).AsScalar());
        }
        internal EngineEffectVariableVector GetVariableVector(string name)
        {
            return new EngineEffectVariableVector(this.effect.GetVariableByName(name).AsVector());
        }
        internal EngineEffectVariableMatrix GetVariableMatrix(string name)
        {
            return new EngineEffectVariableMatrix(this.effect.GetVariableByName(name).AsMatrix());
        }

        public void Dispose()
        {
            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }
        }
    }
}
