using Engine.BuiltIn.Format;
using Engine.Common;

namespace Engine.BuiltIn.Drawers.Forward
{
    /// <summary>
    /// Drawer manager
    /// </summary>
    public static class ForwardDrawerManager
    {
        /// <summary>
        /// Gets the drawing effect for the specified mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="instanced">Use instancing data</param>
        /// <returns>Returns the drawing effect</returns>
        public static IDrawer GetDrawer(IMesh mesh, bool instanced)
        {
            var instance = mesh.GetVertexType();
            if (instance == null)
            {
                return null;
            }

            return GetDrawer(instance, instanced);
        }
        /// <summary>
        /// Gets the drawing effect for the specified vertex data type
        /// </summary>
        /// <typeparam name="T">Vertex data type</typeparam>
        /// <param name="instanced">Use instancing data</param>
        /// <returns>Returns the drawing effect</returns>
        public static IDrawer GetDrawer<T>(bool instanced) where T : struct, IVertexData
        {
            return GetDrawer(default(T), instanced);
        }

        private static IDrawer GetDrawer<T>(T instance, bool instanced) where T : IVertexData
        {
            if (instanced)
            {
                return GetDrawerInstanced(instance);
            }

            return GetDrawerSingle(instance);
        }
        private static IDrawer GetDrawerSingle<T>(T instance) where T : IVertexData
        {
            return instance switch
            {
                VertexPositionColor => BuiltInShaders.GetDrawer<BuiltInPositionColor>(),
                VertexPositionTexture => BuiltInShaders.GetDrawer<BuiltInPositionTexture>(),
                VertexPositionNormalColor => BuiltInShaders.GetDrawer<BuiltInPositionNormalColor>(),
                VertexPositionNormalTexture => BuiltInShaders.GetDrawer<BuiltInPositionNormalTexture>(),
                VertexPositionNormalTextureTangent => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangent>(),
                VertexSkinnedPositionColor => BuiltInShaders.GetDrawer<BuiltInPositionColorSkinned>(),
                VertexSkinnedPositionTexture => BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinned>(),
                VertexSkinnedPositionNormalColor => BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinned>(),
                VertexSkinnedPositionNormalTexture => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinned>(),
                VertexSkinnedPositionNormalTextureTangent => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinned>(),
                _ => null,
            };
        }
        private static IDrawer GetDrawerInstanced<T>(T instance) where T : IVertexData
        {
            return instance switch
            {
                VertexPositionColor => BuiltInShaders.GetDrawer<BuiltInPositionColorInstanced>(),
                VertexPositionTexture => BuiltInShaders.GetDrawer<BuiltInPositionTextureInstanced>(),
                VertexPositionNormalColor => BuiltInShaders.GetDrawer<BuiltInPositionNormalColorInstanced>(),
                VertexPositionNormalTexture => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureInstanced>(),
                VertexPositionNormalTextureTangent => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentInstanced>(),
                VertexSkinnedPositionColor => BuiltInShaders.GetDrawer<BuiltInPositionColorSkinnedInstanced>(),
                VertexSkinnedPositionTexture => BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinnedInstanced>(),
                VertexSkinnedPositionNormalColor => BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinnedInstanced>(),
                VertexSkinnedPositionNormalTexture => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinnedInstanced>(),
                VertexSkinnedPositionNormalTextureTangent => BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinnedInstanced>(),
                _ => null,
            };
        }
    }
}
