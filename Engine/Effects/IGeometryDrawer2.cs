using SharpDX;
using System.Collections.Generic;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Geometry drawer interface
    /// </summary>
    public interface IGeometryDrawer2
    {
        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWidth">Animation palette texture width</param>
        void UpdateGlobals(
            EngineShaderResourceView materialPalette,
            uint materialPaletteWidth,
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth);
        /// <summary>
        /// Update per frame full data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="context">Context</param>
        void UpdatePerFrame(Matrix world, DrawContext context);
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animation">Animation information</param>
        /// <param name="material">Material information</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="tintColor">Tint color</param>
        void UpdatePerObject(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex,
            Color4 tintColor);

        /// <summary>
        /// Draws the specified meshes shadow maps
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="meshes">Mesh list</param>
        void DrawShadows(BufferManager bufferManager, IEnumerable<Mesh> meshes);
        /// <summary>
        /// Draws the specified skinned meshes shadow maps
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="meshes">Mesh list</param>
        void DrawShadowsSkinned(BufferManager bufferManager, IEnumerable<Mesh> meshes);
        /// <summary>
        /// Draws the specified meshes
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="meshes">Mesh list</param>
        void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes);
        /// <summary>
        /// Draws the specified skinned meshes
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="meshes">Mesh list</param>
        void DrawSkinned(BufferManager bufferManager, IEnumerable<Mesh> meshes);
    }
}
