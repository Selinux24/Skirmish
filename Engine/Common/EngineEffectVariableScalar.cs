
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    class EngineEffectVariableScalar
    {
        private EffectScalarVariable variable = null;

        public EngineEffectVariableScalar(EffectScalarVariable variable)
        {
            this.variable = variable;
        }

        public float GetFloat()
        {
            return this.variable.GetFloat();
        }
        public int GetInt()
        {
            return this.variable.GetInt();
        }
        public bool GetBool()
        {
            return this.variable.GetBool();
        }

        public void Set(bool value)
        {
            this.variable.Set(value);
        }
        public void Set(int value)
        {
            this.variable.Set(value);
        }
        public void Set(float value)
        {
            this.variable.Set(value);
        }
    }
}
