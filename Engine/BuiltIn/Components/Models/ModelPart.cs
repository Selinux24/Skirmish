using SharpDX;

namespace Engine.BuiltIn.Components.Models
{
    /// <summary>
    /// Model part
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Name</param>
    public class ModelPart(string name) : IModelPart
    {
        /// <inheritdoc/>
        public string Name { get; set; } = name;
        /// <inheritdoc/>
        public Matrix InitialWorldTransform { get; private set; } = Matrix.Identity;
        /// <inheritdoc/>
        public Matrix InitialLocalTransform
        {
            get
            {
                // Get parent's initial transform
                var parentTransform = Parent?.InitialWorldTransform ?? Matrix.Identity;

                // Transform to local
                return Matrix.Invert(parentTransform) * InitialWorldTransform;
            }
        }
        /// <inheritdoc/>
        public IModelPart Parent { get; private set; }
        /// <inheritdoc/>
        public IManipulator3D Manipulator { get; private set; } = new Manipulator3D();

        /// <inheritdoc/>
        public void SetParent(IModelPart parent)
        {
            Parent = parent;
        }
        /// <inheritdoc/>
        public void SetWorldTransform(Matrix transform)
        {
            InitialWorldTransform = transform;
        }
        /// <inheritdoc/>
        public Matrix GetWorldTransform()
        {
            // Build transform
            var local = GetParentManipulatorTransform();

            local *= InitialLocalTransform;

            return (Parent?.InitialWorldTransform ?? Matrix.Identity) * local;
        }
        /// <inheritdoc/>
        public Matrix GetLocalTransform()
        {
            // Get the parent transform
            return Manipulator.LocalTransform * (Parent?.GetLocalTransform() ?? Matrix.Identity);
        }
        /// <inheritdoc/>
        public Matrix GetPartTransform()
        {
            // Calculate the part's world transform
            var worldTransform = Matrix.Invert(InitialWorldTransform) * Manipulator.LocalTransform * InitialWorldTransform;

            // Get the parent's part world transform, if any
            var parentTransform = Parent?.GetPartTransform() ?? Matrix.Identity;

            // Build transform - 
            return worldTransform * parentTransform;
        }
        /// <inheritdoc/>
        public Matrix GetParentManipulatorTransform()
        {
            return Manipulator.LocalTransform * (Parent?.GetParentManipulatorTransform() ?? Matrix.Identity);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}. {Manipulator}";
        }
    }
}
