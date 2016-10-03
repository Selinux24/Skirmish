﻿
namespace Engine
{
    /// <summary>
    /// Instanced model description
    /// </summary>
    public class ModelInstancedDescription
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name = null;
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
        /// Instances
        /// </summary>
        public int Instances = 1;
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow { get; set; }
        /// <summary>
        /// Is Static
        /// </summary>
        public bool Static = false;
        /// <summary>
        /// Can be renderer by the deferred renderer
        /// </summary>
        public bool DeferredEnabled = true;
    }
}
