using Engine.Common;

namespace Engine.BuiltIn.Deferred
{
    /// <summary>
    /// Drawer manager
    /// </summary>
    public static class DeferredDrawerManager
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
            switch (vertexType)
            {
                case VertexTypes.PositionColor:
                    return BuiltInShaders.GetDrawer<BuiltInPositionColor>();
                case VertexTypes.PositionTexture:
                    return BuiltInShaders.GetDrawer<BuiltInPositionTexture>();
                case VertexTypes.PositionNormalColor:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalColor>();
                case VertexTypes.PositionNormalTexture:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTexture>();
                case VertexTypes.PositionNormalTextureTangent:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangent>();
                case VertexTypes.PositionColorSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionColorSkinned>();
                case VertexTypes.PositionTextureSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinned>();
                case VertexTypes.PositionNormalColorSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinned>();
                case VertexTypes.PositionNormalTextureSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinned>();
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinned>();
                default:
                    return null;
            }
        }
        /// <summary>
        /// Gets a instanced drawer
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        private static IBuiltInDrawer GetDrawerInstanced(VertexTypes vertexType)
        {
            switch (vertexType)
            {
                case VertexTypes.PositionColor:
                    return BuiltInShaders.GetDrawer<BuiltInPositionColorInstanced>();
                case VertexTypes.PositionTexture:
                    return BuiltInShaders.GetDrawer<BuiltInPositionTextureInstanced>();
                case VertexTypes.PositionNormalColor:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalColorInstanced>();
                case VertexTypes.PositionNormalTexture:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureInstanced>();
                case VertexTypes.PositionNormalTextureTangent:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentInstanced>();
                case VertexTypes.PositionColorSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionColorSkinnedInstanced>();
                case VertexTypes.PositionTextureSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinnedInstanced>();
                case VertexTypes.PositionNormalColorSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinnedInstanced>();
                case VertexTypes.PositionNormalTextureSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinnedInstanced>();
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinnedInstanced>();
                default:
                    return null;
            }
        }
    }
}
