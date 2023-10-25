using SharpDX;

namespace Engine
{
    /// <summary>
    /// Model part
    /// </summary>
    public class ModelPart : IModelPart
    {
        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public Matrix InitialTransform { get; set; } = Matrix.Identity;
        /// <inheritdoc/>
        public IModelPart Parent { get; private set; }
        /// <inheritdoc/>
        public Manipulator3D Manipulator { get; private set; } = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelPart(string name)
        {
            Name = name;
        }

        /// <inheritdoc/>
        public void SetParent(IModelPart parent)
        {
            Parent = parent;
        }
        /// <inheritdoc/>
        public Matrix GetLocalTransform()
        {
            // Calculate local transform
            var localTransform = Matrix.Invert(InitialTransform) * Manipulator.LocalTransform * InitialTransform;

            // Get the parent transform, if any
            var parentTransform = Parent?.GetLocalTransform() ?? Matrix.Identity;

            // Build transform
            return localTransform * parentTransform;
        }
        /// <inheritdoc/>
        public Matrix GetGlobalTransform()
        {
            var transform = Manipulator.LocalTransform * InitialTransform;

            // Get the parent transform, if any
            var parentTransform = Parent?.GetGlobalTransform() ?? Matrix.Identity;

            // Build transform
            return transform * parentTransform;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}. {Manipulator}";
        }
    }
}
