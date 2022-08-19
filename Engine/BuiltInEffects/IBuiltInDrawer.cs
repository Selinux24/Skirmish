using SharpDX;
using System.Collections.Generic;

namespace Engine.BuiltInEffects
{
    using Engine.Common;

    /// <summary>
    /// Built-in drawer interface
    /// </summary>
    public interface IBuiltInDrawer
    {
        /// <summary>
        /// Update object data
        /// </summary>
        /// <param name="material">Material information</param>
        /// <param name="tintColor">Tint color</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="animation">Animation information</param>
        void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation);
        /// <summary>
        /// Draws the specified meshes
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="meshes">Mesh list</param>
        /// <param name="instances">Number of instances</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes, int instances = 0, int startInstanceLocation = 0);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="options">Draw options</param>
        void Draw(BufferManager bufferManager, DrawOptions options);
    }
}
