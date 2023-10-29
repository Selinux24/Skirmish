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
            // Get local transform from manipulator
            var localTransform = Parent == null ? Matrix.Identity : Manipulator.LocalTransform;

            // Calculate local transform
            //localTransform = Matrix.Invert(InitialWorldTransform) * localTransform * InitialWorldTransform;

            // Get the parent transform, if any
            var parentTransform = Parent?.GetLocalTransform() ?? Matrix.Identity;

            // Build transform
            return localTransform * parentTransform;
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
