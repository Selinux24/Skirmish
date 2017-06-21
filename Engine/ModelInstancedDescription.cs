
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Instanced model description
    /// </summary>
    public class ModelInstancedDescription : ModelBaseDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ModelInstancedDescription()
            : base()
        {
            this.Instanced = true;
        }
    }
}
