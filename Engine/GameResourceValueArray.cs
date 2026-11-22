using Engine.Common;

namespace Engine
{
    /// <summary>
    /// Value array resource request
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class GameResourceValueArray<T>(string name, T[] values, uint size, bool dynamic) : IGameResourceRequest where T : unmanaged
    {
        /// <summary>
        /// Values
        /// </summary>
        private readonly T[] values = values;
        /// <summary>
        /// Size
        /// </summary>
        private readonly uint size = size;
        /// <summary>
        /// Dynamic resource
        /// </summary>
        private readonly bool Dynamic = dynamic;

        /// <inheritdoc/>
        public string Name { get; private set; } = name;
        /// <inheritdoc/>
        public EngineShaderResourceView ResourceView { get; private set; } = new EngineShaderResourceView(name);

        /// <inheritdoc/>
        public void Create(Game game)
        {
            var srv = game.Graphics.CreateValueArrayTexture(Name, size, values, Dynamic).GetResource();
            ResourceView.SetResource(srv);
        }
    }
}
