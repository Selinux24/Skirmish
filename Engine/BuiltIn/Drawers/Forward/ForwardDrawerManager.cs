using Engine.BuiltIn.Primitives;

namespace Engine.BuiltIn.Drawers.Forward
{
    /// <summary>
    /// Drawer manager
    /// </summary>
    public static class ForwardDrawerManager
    {
        /// <summary>
        /// Gets the drawing effect for the current instance
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <returns>Returns the drawing effect</returns>
        public static IBuiltInDrawer GetDrawer(VertexTypes vertexType, bool instanced)
        {
            if (instanced)
            {
                return GetDrawerInstanced(vertexType);
            }

            return GetDrawerSingle(vertexType);
        }
        /// <summary>
        /// Gets a single drawer
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        private static IBuiltInDrawer GetDrawerSingle(VertexTypes vertexType)
        {
            return vertexType switch
            {
                VertexTypes.PositionColor => BuiltInShaders.GetDrawer<BuiltInPositionColor>(),
                VertexTypes.PositionTexture => BuiltInShaders.GetDrawer<BuiltInPositionTexture>(),
                VertexTypes.PositionNormalColor => BuiltInShaders.GetDrawer<BuiltInPositionNormalColor>(),
                VertexTypes.PositionNormalTexture => BuiltInShaders.GetDrawer<BuiltInPositionNormalTexture>(),
                VertexTypes.PositionNormalTextureTangent => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangent>(),
                VertexTypes.PositionColorSkinned => BuiltInShaders.GetDrawer<BuiltInPositionColorSkinned>(),
                VertexTypes.PositionTextureSkinned => BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinned>(),
                VertexTypes.PositionNormalColorSkinned => BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinned>(),
                VertexTypes.PositionNormalTextureSkinned => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinned>(),
                VertexTypes.PositionNormalTextureTangentSkinned => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinned>(),
                _ => null,
            };
        }
        /// <summary>
        /// Gets a instanced drawer
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        private static IBuiltInDrawer GetDrawerInstanced(VertexTypes vertexType)
        {
            return vertexType switch
            {
                VertexTypes.PositionColor => BuiltInShaders.GetDrawer<BuiltInPositionColorInstanced>(),
                VertexTypes.PositionTexture => BuiltInShaders.GetDrawer<BuiltInPositionTextureInstanced>(),
                VertexTypes.PositionNormalColor => BuiltInShaders.GetDrawer<BuiltInPositionNormalColorInstanced>(),
                VertexTypes.PositionNormalTexture => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureInstanced>(),
                VertexTypes.PositionNormalTextureTangent => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentInstanced>(),
                VertexTypes.PositionColorSkinned => BuiltInShaders.GetDrawer<BuiltInPositionColorSkinnedInstanced>(),
                VertexTypes.PositionTextureSkinned => BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinnedInstanced>(),
                VertexTypes.PositionNormalColorSkinned => BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinnedInstanced>(),
                VertexTypes.PositionNormalTextureSkinned => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinnedInstanced>(),
                VertexTypes.PositionNormalTextureTangentSkinned => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinnedInstanced>(),
                _ => null,
            };
        }
    }
}
