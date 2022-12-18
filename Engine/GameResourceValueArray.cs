using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Value array resource request
    /// </summary>
    public class GameResourceValueArray<T> : IGameResourceRequest where T : struct
    {
        /// <inheritdoc/>
        public string Name { get; private set; }
        /// <inheritdoc/>
        public EngineShaderResourceView ResourceView { get; private set; }
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

        /// <summary>
        /// Constructor
        /// </summary>
        public GameResourceValueArray(string name)
        {
            Name = name;
            ResourceView = new EngineShaderResourceView(name);
        }

        /// <inheritdoc/>
        public void Create(Game game)
        {
            var srv = game.Graphics.CreateValueArrayTexture(Name, Size, Values, Dynamic).GetResource();
            ResourceView.SetResource(srv);
        }
    }
}
