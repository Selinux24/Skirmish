using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Value array resource request
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class GameResourceValueArray<T>(string name) : IGameResourceRequest where T : struct
    {
        /// <inheritdoc/>
        public string Name { get; private set; } = name;
        /// <inheritdoc/>
        public EngineShaderResourceView ResourceView { get; private set; } = new EngineShaderResourceView(name);
        /// <summary>
        /// Size
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Values
        /// </summary>
        public IEnumerable<T> Values { get; set; }
        /// <summary>
        /// Dynamic resource
        /// </summary>
        public bool Dynamic { get; set; }

        /// <inheritdoc/>
        public void Create(Game game)
        {
            var srv = game.Graphics.CreateValueArrayTexture(Name, Size, Values, Dynamic).GetResource();
            ResourceView.SetResource(srv);
        }
    }
}
