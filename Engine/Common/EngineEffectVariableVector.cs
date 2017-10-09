
namespace Engine.Common
{
    using SharpDX;
    using SharpDX.Direct3D11;

    class EngineEffectVariableVector
    {
        private EffectVectorVariable variable = null;

        public EngineEffectVariableVector(EffectVectorVariable variable)
        {
            this.variable = variable;
        }

        public Vector4 GetFloatVector()
        {
            return this.variable.GetFloatVector();
        }
        public Int4 GetIntVector()
        {
            return this.variable.GetIntVector();
        }
        public T GetVector<T>() where T : struct
        {
            return this.variable.GetVector<T>();
        }

        public void Set(Vector4 value)
        {
            this.variable.Set(value);
        }
        public void Set(Int4 value)
        {
            this.variable.Set(value);
        }
    }
}
