using SharpDX;

namespace Engine
{
    /// <summary>
    /// Game cursor
    /// </summary>
    public class GameCursor : Sprite
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="scene">Scene</param>
        /// <param name="texture">Texture</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public GameCursor(Game game, Scene3D scene, string texture, float width, float height)
            : base(game, scene, texture, width, height)
        {
            this.Game.Input.VisibleMouse = false;
        }

        /// <summary>
        /// Update cursor state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Update(GameTime gameTime)
        {
            this.Manipulator.SetPosition(new Vector2(this.Game.Input.MouseX, this.Game.Input.MouseY));

            base.Update(gameTime);
        }
    }
}
