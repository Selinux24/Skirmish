
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Random texture resource request
    /// </summary>
    public class GameResourceRandomTexture : IGameResourceRequest
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
        /// Creates the resource
        /// </summary>
        /// <param name="game">Game</param>
        public void Create(Game game)
        {
            var srv = game.Graphics.CreateRandomTexture(Size, Min, Max, Seed);
            ResourceView.SetResource(srv);
        }
    }
}
