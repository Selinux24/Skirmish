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
        /// Initial part transform
        /// </summary>
        Matrix InitialTransform { get; set; }
        /// <summary>
        /// Parent model part
        /// </summary>
        IModelPart Parent { get; }
        /// <summary>
        /// Manipulator
        /// </summary>
        Manipulator3D Manipulator { get; }

        /// <summary>
        /// Sets the parent model part
        /// </summary>
        /// <param name="parent">Parent</param>
        void SetParent(IModelPart parent);
        /// <summary>
        /// Gets the part transform
        /// </summary>
        Matrix GetTransform();
    }
}
