using SharpDX;

namespace Engine.Animation
{
    /// <summary>
    /// Skinning data
    /// </summary>
    public class SkinningData
    {
        /// <summary>
        /// Default clip name
        /// </summary>
        public const string DefaultClip = "default";

        /// <summary>
        /// Skeleton
        /// </summary>
        public Skeleton Skeleton = null;
        /// <summary>
        /// Animation velocity modifier
        /// </summary>
        public float AnimationVelocity = 1f;
        /// <summary>
        /// Current clip name
        /// </summary>
        public string ClipName { get; private set; }
        /// <summary>
        /// Clip position
        /// </summary>
        public float Time { get; set; }
        /// <summary>
        /// Gets or sets if clip must be looped
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        public SkinningData(Skeleton skeleton)
        {
            this.Skeleton = skeleton;
            this.Time = 0;
            this.ClipName = DefaultClip;
            this.Loop = true;
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            if (this.ClipName != null)
            {
                this.Skeleton.Update(this.Time, this.ClipName);
            }

            this.Time += gameTime.ElapsedSeconds;
        }
        /// <summary>
        /// Sets clip to play
        /// </summary>
        /// <param name="clipName">Clip name</param>
        public void SetClip(string clipName)
        {
            this.ClipName = clipName;
        }
        /// <summary>
        /// Gets final transform collection
        /// </summary>
        /// <returns>Returns final transform collection</returns>
        public Matrix[] GetFinalTransforms()
        {
            return this.Skeleton.FinalTransforms;
        }

        /// <summary>
        /// Gets text representation of skinning data instance
        /// </summary>
        /// <returns>Returns text representation of skinning data instance</returns>
        public override string ToString()
        {
            return string.Format("{0}(Loop: {1}) -> {2}", this.ClipName, this.Loop, this.Time);
        }
    }
}
