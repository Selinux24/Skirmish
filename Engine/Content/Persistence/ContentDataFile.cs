using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Engine.Content.Persistence
{
    /// <summary>
    /// Model content description
    /// </summary>
    public class ContentDataFile
    {
        /// <summary>
        /// Reads the content data from disk
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="filename">File name</param>
        public static async Task<IEnumerable<ContentData>> ReadContentData(string contentFolder, string filename)
        {
            string resourceFile = Path.GetFileName(filename);
            string resourceFolder = Path.Combine(contentFolder ?? string.Empty, Path.GetDirectoryName(filename));
            string resourceExt = Path.GetExtension(filename);

            ContentDataFile contentData;

            if (resourceExt != ".json")
            {
                contentData = new ContentDataFile()
                {
                    ModelFileName = resourceFile,
                };
            }
            else
            {
                contentData = SerializationHelper.DeserializeFromFile<ContentDataFile>(Path.Combine(resourceFolder, resourceFile));
            }

            return await ReadContentData(resourceFolder, contentData);
        }
        /// <summary>
        /// Reads the content data from a content data file descriptor
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="contentData">Content data file descriptor</param>
        public static async Task<IEnumerable<ContentData>> ReadContentData(string contentFolder, ContentDataFile contentData)
        {
            var loader = GameResourceManager.GetLoaderForFile(contentData.ModelFileName);
            return loader == null
                ? throw new EngineException($"No loader found for file '{contentData.ModelFileName}'")
                : await loader.Load(contentFolder ?? string.Empty, contentData);
        }

        /// <summary>
        /// Model file name
        /// </summary>
        public string ModelFileName { get; set; } = null;

        /// <summary>
        /// Position vector
        /// </summary>
        public Position3 Position { get; set; } = Position3.Zero;
        /// <summary>
        /// Rotation
        /// </summary>
        public RotationQ Rotation { get; set; } = RotationQ.Identity;
        /// <summary>
        /// Rotation quaternion
        /// </summary>
        /// <summary>
        /// Scale
        /// </summary>
        public Scale3 Scale { get; set; } = Scale3.One;

        /// <summary>
        /// Meshes by level of detail in the file
        /// </summary>
        /// <remarks>For model files containing several levels of details for the same model</remarks>
        public string[] LODMeshes { get; set; } = null;
        /// <summary>
        /// Hull meshes collection
        /// </summary>
        public string[] HullMeshes { get; set; } = null;

        /// <summary>
        /// Read animations
        /// </summary>
        public bool ReadAnimations { get; set; } = true;
        /// <summary>
        /// Animation description
        /// </summary>
        public AnimationFile Animation { get; set; } = null;
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
        /// Gets the initial transform matrix
        /// </summary>
        public Matrix4X4 GetTransform()
        {
            return
                Matrix4X4.Scaling(Scale) *
                Matrix4X4.Rotation(Rotation) *
                Matrix4X4.Translation(Position);
        }
    }
}
