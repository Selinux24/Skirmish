
namespace Engine.Common
{
    using SharpDX;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Matrix variable
    /// </summary>
    public class EngineEffectVariableMatrix
    {
        /// <summary>
        /// Effect matrix variable
        /// </summary>
        private EffectMatrixVariable variable = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="variable">Internal effect variable</param>
        public EngineEffectVariableMatrix(EffectMatrixVariable variable)
        {
            this.variable = variable;
        }

        /// <summary>
        /// Gets a matrix value
        /// </summary>
        /// <returns>Returns the matrix value from the variable</returns>
        public Matrix GetMatrix()
        {
            return this.variable.GetMatrix();
        }

        /// <summary>
        /// Sets a matrix value to the variable
        /// </summary>
        /// <param name="value">Value</param>
        public void SetMatrix(Matrix value)
        {
            this.variable.SetMatrix(value);
        }
    }
}
