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
        void UpdatePerObject();
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="material">Material information</param>
        /// <param name="tintColor">Tint color</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="animation">Animation information</param>
        void UpdatePerObject(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation);
    }
}
