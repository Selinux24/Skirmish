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
        /// <param name="animation">Animation information</param>
        /// <param name="material">Material information</param>
        /// <param name="textureIndex">Texture index</param>
        void UpdatePerObject(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex);
    }

    /// <summary>
    /// Material draw information
    /// </summary>
    public struct MaterialDrawInfo
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly MaterialDrawInfo Empty = new MaterialDrawInfo();

        /// <summary>
        /// Material
        /// </summary>
        public IMeshMaterial Material { get; set; }
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic { get; set; }
    }

    /// <summary>
    /// Animation draw information
    /// </summary>
    public struct AnimationDrawInfo
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly AnimationDrawInfo Empty = new AnimationDrawInfo();

        /// <summary>
        /// First offset in the animation palette
        /// </summary>
        public uint Offset1 { get; set; }
        /// <summary>
        /// Second offset in the animation palette
        /// </summary>
        public uint Offset2 { get; set; }
        /// <summary>
        /// Interpolation amount between the offsets
        /// </summary>
        public float InterpolationAmount { get; set; }
    }
}
