
namespace Engine.Common
{
    using SharpDX;
    using SharpDX.Direct3D11;

    class EngineEffectVariableMatrix
    {
        private EffectMatrixVariable variable = null;

        public EngineEffectVariableMatrix(EffectMatrixVariable variable)
        {
            this.variable = variable;
        }

        public Matrix GetMatrix()
        {
            return this.variable.GetMatrix();
        }

        public void SetMatrix(Matrix value)
        {
            this.variable.SetMatrix(value);
        }
    }
}
