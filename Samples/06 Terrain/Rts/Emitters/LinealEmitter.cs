using Engine;
using SharpDX;

namespace Terrain.Rts.Emitters
{
    /// <summary>
    /// Linear emitter
    /// </summary>
    public class LinealEmitter : ParticleEmitter
    {
        private readonly Vector3 to;
        private readonly Vector3 direction;
        private readonly float speed;
        private float previousDistance = float.MaxValue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="from">From position</param>
        /// <param name="to">To position</param>
        /// <param name="speed">Speed</param>
        public LinealEmitter(Vector3 from, Vector3 to, float speed) : base()
        {
            this.to = to;
            var vDir = to - from;
            direction = Vector3.Normalize(vDir);
            this.speed = speed;

            Position = from;
            Duration = vDir.Length() / speed;
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime, Vector3 pointOfView)
        {
            float distance = Vector3.Distance(to, Position);

            if (distance < previousDistance)
            {
                Position += direction * speed * gameTime.ElapsedSeconds;

                previousDistance = distance;
            }

            base.Update(gameTime, pointOfView);
        }
    }
}
