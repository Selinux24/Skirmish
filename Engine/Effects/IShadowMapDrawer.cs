using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Shadow map effect interface
    /// </summary>
    public interface IShadowMapDrawer : IDrawer
    {
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="transparent">Use transparent textures</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, bool transparent);

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        void UpdateGlobals(EngineShaderResourceView animationPalette, uint animationPaletteWidth);
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="context">Context</param>
        void UpdatePerFrame(Matrix world, DrawContextShadows context);
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        void UpdatePerObject(uint animationOffset, IMeshMaterial material, uint textureIndex);
    }
}
