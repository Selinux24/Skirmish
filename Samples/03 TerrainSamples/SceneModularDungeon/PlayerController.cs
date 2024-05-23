using Engine;
using Engine.PathFinding;
using SharpDX;
using System;

namespace TerrainSamples.SceneModularDungeon
{
    /// <summary>
    /// Player controller
    /// </summary>
    /// <param name="scene">Scene</param>
    /// <param name="player">Player</param>
    /// <remarks>
    /// Head sway algorithm taken from https://www.youtube.com/watch?v=2ysd9uWmUfo
    /// Thanks to https://www.youtube.com/@gamedevinspire channel
    /// </remarks>
    internal class PlayerController(WalkableScene scene, Player player)
    {
        /// <summary>
        /// Walkable scene
        /// </summary>
        private readonly WalkableScene scene = scene;
        /// <summary>
        /// Player
        /// </summary>
        private readonly Player player = player;
        /// <summary>
        /// Player transform
        /// </summary>
        private readonly Manipulator3D transform = new();
        /// <summary>
        /// Previous position
        /// </summary>
        private Vector3 prevPosition;
        /// <summary>
        /// Head position delta
        /// </summary>
        private Vector3 headPositionDelta;

        /// <summary>
        /// Sway amount
        /// </summary>
        public float Amount { get; set; } = 0.01f;
        /// <summary>
        /// Sway frequency
        /// </summary>
        public float Frequency { get; set; } = 10f;
        /// <summary>
        /// Sway smooth
        /// </summary>
        public float Smooth { get; set; } = 10f;
        /// <summary>
        /// Sway scale
        /// </summary>
        public float Scale { get; set; } = 15f;
        /// <summary>
        /// Gets the player position
        /// </summary>
        public Vector3 Position { get => transform.Position; }
        /// <summary>
        /// Gets the player forward direction
        /// </summary>
        public Vector3 Direction { get => -transform.Forward; }
        /// <summary>
        /// Gets the player left direction
        /// </summary>
        public Vector3 Left { get => transform.Left; }
        /// <summary>
        /// Gets the player up direction
        /// </summary>
        public Vector3 Up { get => transform.Up; }

        /// <summary>
        /// Initializes the player position and interest
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="interest">Interest</param>
        public void Initialize(Vector3 position, Vector3 interest)
        {
            prevPosition = position;
            transform.SetPosition(position);
            transform.LookAt(interest);
        }

        /// <summary>
        /// Updates the player state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(IGameTime gameTime)
        {
            prevPosition = transform.Position;

            var input = scene.Game.Input;
            float vel = input.ShiftPressed ? player.VelocitySlow : player.Velocity;
            float velX = input.MouseXDelta * 0.1f;
            float velY = input.MouseYDelta * 0.1f;

            if (input.KeyPressed(Keys.A))
            {
                transform.MoveLeft(gameTime, vel);
            }

            if (input.KeyPressed(Keys.D))
            {
                transform.MoveRight(gameTime, vel);
            }

            if (input.KeyPressed(Keys.W))
            {
                transform.MoveForward(gameTime, vel);
            }

            if (input.KeyPressed(Keys.S))
            {
                transform.MoveBackward(gameTime, vel);
            }

#if DEBUG
            if (input.MouseButtonPressed(MouseButtons.Right))
            {
                transform.Rotate(gameTime, velX, -velY, 0);
            }
#else
            Transform.Rotate(gameTime, velX, -velY, 0);
#endif

            transform.Update(gameTime);

            if (scene.Walk(player, prevPosition, transform.Position, true, out var walkerPos))
            {
                transform.SetPosition(walkerPos);
            }
            else
            {
                transform.SetPosition(prevPosition);
            }

            UpdateHeadSway(gameTime);

            var position = transform.Position + headPositionDelta;
            var interest = position - transform.Forward;

            scene.Camera.SetPosition(position);
            scene.Camera.SetInterest(interest);
        }
        /// <summary>
        /// Updates the head sway
        /// </summary>
        /// <param name="gameTime">Game time</param>
        private void UpdateHeadSway(IGameTime gameTime)
        {
            float totalTime = gameTime.TotalSeconds;
            float dt = gameTime.ElapsedSeconds;

            float v = (transform.Position - prevPosition).LengthSquared();
            if (v > 0)
            {
                StartHeadSway(totalTime, dt);
            }

            StopHeadSway(dt);
        }
        /// <summary>
        /// Starts the head sway
        /// </summary>
        /// <param name="totalTime">Total time</param>
        /// <param name="dt">Delta time</param>
        private void StartHeadSway(float totalTime, float dt)
        {
            var pos = Vector3.Zero;
            pos.Y = MathUtil.Lerp(pos.Y, MathF.Sin(totalTime * Frequency) * Amount * 1.4f, Smooth * dt);
            pos.X = MathUtil.Lerp(pos.X, MathF.Cos(totalTime * Frequency * 0.5f) * Amount * 1.6f, Smooth * dt);
            headPositionDelta = pos * Scale;
        }
        /// <summary>
        /// Stops the head sway
        /// </summary>
        /// <param name="dt">Delta time</param>
        private void StopHeadSway(float dt)
        {
            if (Vector3.NearEqual(headPositionDelta, Vector3.Zero, new(0.0001f)))
            {
                return;
            }

            headPositionDelta = Vector3.Lerp(headPositionDelta, Vector3.Zero, dt);
        }
    }
}
