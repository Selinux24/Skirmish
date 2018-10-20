
namespace Engine
{
    /// <summary>
    /// Model part
    /// </summary>
    public class ModelPart
    {
        /// <summary>
        /// Part name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelPart(string name)
        {
            this.Name = name;
            this.Manipulator = new Manipulator3D();
        }
    }
}
