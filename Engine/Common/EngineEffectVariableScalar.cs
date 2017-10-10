
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Scalar variable
    /// </summary>
    public class EngineEffectVariableScalar
    {
        /// <summary>
        /// Effect scalar variable
        /// </summary>
        private EffectScalarVariable variable = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="variable">Internal variable</param>
        public EngineEffectVariableScalar(EffectScalarVariable variable)
        {
            this.variable = variable;
        }

        /// <summary>
        /// Gets a float value from the variable
        /// </summary>
        /// <returns>Returns the value from the variable</returns>
        public float GetFloat()
        {
            return this.variable.GetFloat();
        }
        /// <summary>
        /// Gets a int value from the variable
        /// </summary>
        /// <returns>Returns the value from the variable</returns>
        public int GetInt()
        {
            return this.variable.GetInt();
        }
        /// <summary>
        /// Gets a uint value from the variable
        /// </summary>
        /// <returns>Returns the value from the variable</returns>
        public uint GetUInt()
        {
            return (uint)this.variable.GetFloat();
        }
        /// <summary>
        /// Gets a bool value from the variable
        /// </summary>
        /// <returns>Returns the value from the variable</returns>
        public bool GetBool()
        {
            return this.variable.GetBool();
        }

        /// <summary>
        /// Sets a float value to the variable
        /// </summary>
        /// <param name="value">Value</param>
        public void Set(float value)
        {
            this.variable.Set(value);
        }
        /// <summary>
        /// Sets a int value to the variable
        /// </summary>
        /// <param name="value">Value</param>
        public void Set(int value)
        {
            this.variable.Set(value);
        }
        /// <summary>
        /// Sets a uint value to the variable
        /// </summary>
        /// <param name="value">Value</param>
        public void Set(uint value)
        {
            this.variable.Set(value);
        }
        /// <summary>
        /// Sets a bool value to the variable
        /// </summary>
        /// <param name="value">Value</param>
        public void Set(bool value)
        {
            this.variable.Set(value);
        }
    }
}
