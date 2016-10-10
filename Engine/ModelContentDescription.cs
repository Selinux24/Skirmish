
namespace Engine
{
    using Engine.Animation;

    /// <summary>
    /// Model content description
    /// </summary>
    public class ModelContentDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Model file name
        /// </summary>
        public string ModelFileName = null;
        /// <summary>
        /// Volume meshes collection
        /// </summary>
        public string[] VolumeMeshes = null;
        /// <summary>
        /// Animation description
        /// </summary>
        public AnimationDescription Animation = null;
    }
}
