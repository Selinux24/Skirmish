
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Random texture resource request
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class GameResourceRandomTexture(string name) : IGameResourceRequest
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
        /// Minimum value
        /// </summary>
        public float Min { get; set; }
        /// <summary>
        /// Maximum value
        /// </summary>
        public float Max { get; set; }
        /// <summary>
        /// Random seed
        /// </summary>
        public int Seed { get; set; }
        /// <summary>
        /// Dynamic resource
        /// </summary>
        public bool Dynamic { get; set; }

        /// <inheritdoc/>
        public void Create(Game game)
        {
            var srv = game.Graphics.CreateRandomTexture(Name, Size, Min, Max, Seed, Dynamic).GetResource();
            ResourceView.SetResource(srv);
        }
    }
}
