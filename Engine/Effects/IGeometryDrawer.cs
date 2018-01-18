using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Geometry drawer interface
    /// </summary>
    public interface IGeometryDrawer : IDrawer
    {
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced);

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="context">Context</param>
        void UpdatePerFrameBasic(Matrix world, DrawContext context);
        /// <summary>
        /// Update per frame full data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="context">Context</param>
        void UpdatePerFrameFull(Matrix world, DrawContext context);
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="useAnisotropic">Use anisotropic filtering</param>
        void UpdatePerObject(
            uint animationOffset,
            MeshMaterial material,
            uint textureIndex,
            bool useAnisotropic);
    }
}
