using System.Numerics;

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
        public Matrix4x4 InitialWorldTransform { get; private set; } = Matrix4x4.Identity;
        /// <inheritdoc/>
        public Matrix4x4 InitialLocalTransform
        {
            get
            {
                // Get parent's initial transform
                var parentTransform = Parent?.InitialWorldTransform ?? Matrix4x4.Identity;

                // Transform to local
                Matrix4x4.Invert(parentTransform, out var inv);
                return inv * InitialWorldTransform;
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
        public void SetWorldTransform(Matrix4x4 transform)
        {
            InitialWorldTransform = transform;
        }
        /// <inheritdoc/>
        public Matrix4x4 GetWorldTransform()
        {
            // Build transform
            var local = GetParentManipulatorTransform();

            local *= InitialLocalTransform;

            return (Parent?.InitialWorldTransform ?? Matrix4x4.Identity) * local;
        }
        /// <inheritdoc/>
        public Matrix4x4 GetLocalTransform()
        {
            // Get the parent transform
            return Manipulator.LocalTransform * (Parent?.GetLocalTransform() ?? Matrix4x4.Identity);
        }
        /// <inheritdoc/>
        public Matrix4x4 GetPartTransform()
        {
            // Calculate the part's world transform
            Matrix4x4.Invert(InitialWorldTransform, out var inv);
            var worldTransform = inv * Manipulator.LocalTransform * InitialWorldTransform;

            // Get the parent's part world transform, if any
            var parentTransform = Parent?.GetPartTransform() ?? Matrix4x4.Identity;

            // Build transform - 
            return worldTransform * parentTransform;
        }
        /// <inheritdoc/>
        public Matrix4x4 GetParentManipulatorTransform()
        {
            return Manipulator.LocalTransform * (Parent?.GetParentManipulatorTransform() ?? Matrix4x4.Identity);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}. {Manipulator}";
        }
    }
}
