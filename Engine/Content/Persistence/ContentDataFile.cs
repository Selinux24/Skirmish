
namespace Engine.Content.Persistence
{
    using Engine.Animation;

    /// <summary>
    /// Model content description
    /// </summary>
    public class ContentDataFile
    {
        /// <summary>
        /// Model file name
        /// </summary>
        public string ModelFileName { get; set; } = null;
        /// <summary>
        /// Meshes by level of detail in the file
        /// </summary>
        /// <remarks>For model files containing several levels of details for the same model</remarks>
        public string[] LODMeshes { get; set; } = null;
        /// <summary>
        /// Volume meshes collection
        /// </summary>
        public string[] VolumeMeshes { get; set; } = null;
        /// <summary>
        /// Animation description
        /// </summary>
        public AnimationFile Animation { get; set; } = null;
        /// <summary>
        /// Model scale
        /// </summary>
        public float Scale { get; set; } = 1f;
        /// <summary>
        /// Armature name
        /// </summary>
        public string ArmatureName { get; set; } = null;
        /// <summary>
        /// Use controller transforms
        /// </summary>
        public bool UseControllerTransform { get; set; } = true;
        /// <summary>
        /// Bake transforms
        /// </summary>
        public bool BakeTransforms { get; set; } = true;
        /// <summary>
        /// Read animations
        /// </summary>
        public bool ReadAnimations { get; set; } = true;
    }
}
