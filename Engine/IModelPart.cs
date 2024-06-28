using SharpDX;

namespace Engine
{
    /// <summary>
    /// Model part interface
    /// </summary>
    public interface IModelPart
    {
        /// <summary>
        /// Part name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Initial world part transform
        /// </summary>
        Matrix InitialWorldTransform { get; }
        /// <summary>
        /// Initial local part transform
        /// </summary>
        Matrix InitialLocalTransform { get; }
        /// <summary>
        /// Parent model part
        /// </summary>
        IModelPart Parent { get; }
        /// <summary>
        /// Manipulator
        /// </summary>
        IManipulator3D Manipulator { get; }

        /// <summary>
        /// Sets the parent model part
        /// </summary>
        /// <param name="parent">Parent</param>
        void SetParent(IModelPart parent);
        /// <summary>
        /// Sets the world transform
        /// </summary>
        /// <param name="transform">Transform</param>
        void SetWorldTransform(Matrix transform);
        /// <summary>
        /// Gets the part world transform
        /// </summary>
        Matrix GetWorldTransform();
        /// <summary>
        /// Gets the part local transform
        /// </summary>
        Matrix GetLocalTransform();
        /// <summary>
        /// Gets the part's final transform, adding each manipulator local transform
        /// </summary>
        Matrix GetPartTransform();
        /// <summary>
        /// Gets the parent manipulator transform
        /// </summary>
        Matrix GetParentManipulatorTransform();
    }
}
