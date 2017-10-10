
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Vector variable
    /// </summary>
    public class EngineEffectVariableVector
    {
        /// <summary>
        /// Effect vector variable
        /// </summary>
        private EffectVectorVariable variable = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="variable">Effect variable</param>
        public EngineEffectVariableVector(EffectVectorVariable variable)
        {
            this.variable = variable;
        }

        /// <summary>
        /// Gets a value of the specified type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Returns a value of the specified type</returns>
        public T GetVector<T>() where T : struct
        {
            return this.variable.GetVector<T>();
        }
        /// <summary>
        /// Sets a value of the specified type to the variable
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="value">Value</param>
        public void Set<T>(T value) where T : struct
        {
            this.variable.Set<T>(value);
        }
    }
}
