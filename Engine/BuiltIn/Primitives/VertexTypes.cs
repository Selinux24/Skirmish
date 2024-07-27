namespace Engine.BuiltIn.Primitives
{
    /// <summary>
    /// Vertext types enumeration
    /// </summary>
    public enum VertexTypes
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Billboard
        /// </summary>
        Billboard,
        /// <summary>
        /// Font
        /// </summary>
        Font,
        /// <summary>
        /// CPU Particle
        /// </summary>
        CPUParticle,
        /// <summary>
        /// GPU Particles
        /// </summary>
        GPUParticle,
        /// <summary>
        /// Terrain
        /// </summary>
        Terrain,
        /// <summary>
        /// Decal
        /// </summary>
        Decal,

        /// <summary>
        /// Position
        /// </summary>
        Position,
        /// <summary>
        /// Position and color
        /// </summary>
        PositionColor,
        /// <summary>
        /// Position and texture
        /// </summary>
        PositionTexture,
        /// <summary>
        /// Position, normal and color
        /// </summary>
        PositionNormalColor,
        /// <summary>
        /// Position, normal and texture
        /// </summary>
        PositionNormalTexture,
        /// <summary>
        /// Position, normal, texture and tangents
        /// </summary>
        PositionNormalTextureTangent,

        /// <summary>
        /// Position for skinning animation
        /// </summary>
        PositionSkinned,
        /// <summary>
        /// Position and color for skinning animation
        /// </summary>
        PositionColorSkinned,
        /// <summary>
        /// Position and texture for skinning animation
        /// </summary>
        PositionTextureSkinned,
        /// <summary>
        /// Position, normal and color for skinning animation
        /// </summary>
        PositionNormalColorSkinned,
        /// <summary>
        /// Position, normal and texture for skinning animation
        /// </summary>
        PositionNormalTextureSkinned,
        /// <summary>
        /// Position, normal, texture and tangents for skinning animation
        /// </summary>
        PositionNormalTextureTangentSkinned,
    }
}
