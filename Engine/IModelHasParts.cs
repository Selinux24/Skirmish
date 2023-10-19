using SharpDX;

namespace Engine
{
    /// <summary>
    /// Model has parts interface
    /// </summary>
    public interface IModelHasParts
    {
        /// <summary>
        /// Gets the model part count
        /// </summary>
        int ModelPartCount { get; }

        /// <summary>
        /// Gets the transform by transform name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Return the transform of the specified transform name</returns>
        Matrix GetTransformByName(string name);
        /// <summary>
        /// Gets the model part by name
        /// </summary>
        /// <param name="name">Name</param>
        IModelPart GetModelPartByName(string name);
    }
}
