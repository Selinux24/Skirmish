
namespace Engine
{
    using Engine.Mathematics;

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
        Matrix4x4 InitialWorldTransform { get; }
        /// <summary>
        /// Initial local part transform
        /// </summary>
        Matrix4x4 InitialLocalTransform { get; }
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
        void SetWorldTransform(Matrix4x4 transform);
        /// <summary>
        /// Gets the part world transform
        /// </summary>
        Matrix4x4 GetWorldTransform();
        /// <summary>
        /// Gets the part local transform
        /// </summary>
        Matrix4x4 GetLocalTransform();
        /// <summary>
        /// Gets the part's final transform, adding each manipulator local transform
        /// </summary>
        Matrix4x4 GetPartTransform();
        /// <summary>
        /// Gets the parent manipulator transform
        /// </summary>
        Matrix4x4 GetParentManipulatorTransform();
    }
}
