
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
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; } = new Manipulator3D();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelPart(string name)
        {
            Name = name;
        }
    }
}
