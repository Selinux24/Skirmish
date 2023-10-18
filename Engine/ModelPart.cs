using SharpDX;

namespace Engine
{
    /// <summary>
    /// Model part
    /// </summary>
    public class ModelPart : IModelPart
    {
        /// <summary>
        /// Part name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Parent model part
        /// </summary>
        public ModelPart Parent { get; private set; }
        /// <summary>
        /// Initial part transform
        /// </summary>
        public Matrix InitialTransform { get; set; } = Matrix.Identity;
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; } = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelPart(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Sets the parent model part
        /// </summary>
        /// <param name="parent">Parent</param>
        public void SetParent(ModelPart parent)
        {
            Parent = parent;
            Manipulator.Parent = parent?.Manipulator;
        }
        /// <summary>
        /// Gets the part transform
        /// </summary>
        public Matrix GetTransform()
        {
            return Manipulator.GlobalTransform * InitialTransform;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}. {Manipulator}";
        }
    }
}
