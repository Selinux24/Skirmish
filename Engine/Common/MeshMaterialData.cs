using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Mesh material data
    /// </summary>
    public class MeshMaterialData
    {
        /// <summary>
        /// Material content
        /// </summary>
        public IMaterialContent Content { get; set; }
        /// <summary>
        /// Mesh material
        /// </summary>
        public IMeshMaterial Material { get; set; }

        /// <summary>
        /// Create mesh material from material
        /// </summary>
        /// <param name="material">Material</param>
        public static MeshMaterialData FromContent(IMaterialContent material)
        {
            return new MeshMaterialData
            {
                Content = material,
            };
        }

        /// <summary>
        /// Assign textures from texture dictionary to the mesh material
        /// </summary>
        /// <param name="textures">Texture dictionary</param>
        public void AssignTextures(Dictionary<string, MeshTextureData> textures)
        {
            Material = Content.CreateMeshMaterial(textures);
        }
    }
}
