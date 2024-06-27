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
        /// Gets the model part by name
        /// </summary>
        /// <param name="name">Name</param>
        IModelPart GetModelPartByName(string name);

        /// <summary>
        /// Gets the world transform by transform name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Return the world transform of the specified transform name</returns>
        Matrix GetWorldTransformByName(string name);
        /// <summary>
        /// Gets the local transform by transform name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Return the local transform of the specified transform name</returns>
        Matrix GetLocalTransformByName(string name);
        /// <summary>
        /// Gets the pose transform by transform name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Return the pose transform of the specified transform name</returns>
        Matrix GetPoseTransformByName(string name);
        /// <summary>
        /// Gets the final transform by transform name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Return the final transform of the specified transform name</returns>
        Matrix GetPartTransformByName(string name);
    }
}
