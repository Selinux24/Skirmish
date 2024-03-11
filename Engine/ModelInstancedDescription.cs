using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Instanced model description
    /// </summary>
    public class ModelInstancedDescription : BaseModelDescription
    {
        /// <summary>
        /// Transforms
        /// </summary>
        public Matrix[] Transforms { get; set; } = [];
        /// <summary>
        /// Transform names
        /// </summary>
        public string[] TransformNames { get; set; } = [];
        /// <summary>
        /// Transform dependences
        /// </summary>
        public int[] TransformDependences { get; set; } = [];

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelInstancedDescription()
            : base()
        {
            Instanced = true;
        }
    }
}
