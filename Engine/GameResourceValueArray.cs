using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Vector4 value array resource request
    /// </summary>
    public class GameResourceValueArray : IGameResourceRequest
    {
        /// <summary>
        /// Engine resource view
        /// </summary>
        public EngineShaderResourceView ResourceView { get; private set; } = new EngineShaderResourceView();
        /// <summary>
        /// Size
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Vector4 values
        /// </summary>
        public IEnumerable<Vector4> Values { get; set; }
        /// <summary>
        /// Dynamic resource
        /// </summary>
        public bool Dynamic { get; set; }

        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="game">Game</param>
        public void Create(Game game)
        {
            var srv = game.Graphics.CreateTexture2D(Size, Values, Dynamic);
            ResourceView.SetResource(srv);
        }
    }
}
