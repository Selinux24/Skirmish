using Engine;
using Engine.Common;
using SharpDX;

namespace TerrainTest
{
    /// <summary>
    /// Linear emitter
    /// </summary>
    public class LinealEmitter : ParticleEmitter
    {
        private Vector3 from;
        private Vector3 to;
        private float speed;
        private Vector3 direction;
        private float previousDistance = float.MaxValue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="from">From position</param>
        /// <param name="to">To position</param>
        /// <param name="speed">Speed</param>
        public LinealEmitter(Vector3 from, Vector3 to, float speed) : base()
        {
            this.Position = from;
            this.from = from;
            this.to = to;
            this.speed = speed;
            this.direction = (to - from);
            float distance = direction.Length();
            this.direction.Normalize();

            this.Duration = (distance / speed);
        }
    
        /// <summary>
        /// Updates the emitter state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {
            float distance = Vector3.Distance(this.to, this.Position);

            if (distance < previousDistance)
            {
                this.Position += (direction * speed * context.GameTime.ElapsedSeconds);

                this.previousDistance = distance;
            }

            base.Update(context);
        }
    }
}
