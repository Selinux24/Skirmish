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
            ContentDataFile contentData;

            if (Path.GetExtension(filename) != ".json")
            {
                contentData = new ContentDataFile()
                {
                    ModelFileName = filename,
                };
            }
            else
            {
                contentData = SerializationHelper.DeserializeFromFile<ContentDataFile>(Path.Combine(contentFolder, filename));
            }

            var loader = GameResourceManager.GetLoaderForFile(contentData.ModelFileName);

            return await loader.Load(contentFolder, contentData);
        }

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
        /// Hull meshes collection
        /// </summary>
        public string[] HullMeshes { get; set; } = null;
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
