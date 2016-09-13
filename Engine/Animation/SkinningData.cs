using SharpDX;
using System.Collections.Generic;

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
        /// Animations clip dictionary
        /// </summary>
        private Dictionary<string, JointAnimation[]> Animations = null;
        /// <summary>
        /// Current clip name
        /// </summary>
        private string clipName = null;
        /// <summary>
        /// Current animation list
        /// </summary>
        private JointAnimation[] currentAnimations = null;

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
        public string ClipName
        {
            get
            {
                return this.clipName;
            }
            set
            {
                if (this.clipName != value)
                {
                    this.clipName = value;

                    if (this.clipName != null)
                    {
                        this.currentAnimations = this.Animations[this.clipName];
                        this.Duration = this.currentAnimations[0].Duration;
                    }
                    else
                    {
                        this.currentAnimations = null;
                        this.Duration = 0;
                    }
                }
            }
        }
        /// <summary>
        /// Current clip animation duration
        /// </summary>
        public float Duration { get; private set; }
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
        /// <param name="animations">Animation list</param>
        public SkinningData(Skeleton skeleton, JointAnimation[] animations)
        {
            this.Animations = new Dictionary<string, JointAnimation[]>();
            this.Skeleton = skeleton;

            this.Animations.Add(SkinningData.DefaultClip, animations);
            this.Time = 0;
            this.ClipName = DefaultClip;
            this.Loop = true;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="animations">Animation dictionary</param>
        public SkinningData(Skeleton skeleton, Dictionary<string, JointAnimation[]> animations)
        {
            this.Animations = animations;
            this.Skeleton = skeleton;
            this.Time = 0;
            this.ClipName = DefaultClip;
            this.Loop = true;
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            if (this.ClipName != null)
            {
                this.Skeleton.Update(this.Time, this.currentAnimations);

                this.Time += gameTime.ElapsedSeconds;
            }
        }
        /// <summary>
        /// Test animation at time
        /// </summary>
        /// <param name="time">Time</param>
        public void Test(float time)
        {
            if (this.ClipName != null)
            {
                this.Skeleton.Update(time, this.currentAnimations);
            }
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
