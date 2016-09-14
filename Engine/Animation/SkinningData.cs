using SharpDX;
using System.Collections.Generic;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Animation
{
    using Engine.Helpers;

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
        /// Animation clip names collection
        /// </summary>
        public string[] Clips { get; private set; }
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

            this.Time = 0;
            this.Loop = true;
            this.Animations.Add(SkinningData.DefaultClip, animations);
            this.Clips = new string[] { SkinningData.DefaultClip };

            this.ClipName = DefaultClip;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="animations">Animation dictionary</param>
        public SkinningData(Skeleton skeleton, Dictionary<string, JointAnimation[]> animations)
        {
            this.Skeleton = skeleton;

            this.Time = 0;
            this.Loop = true;
            this.Animations = animations;
            this.Clips = this.Animations.Keys.ToArray();

            this.ClipName = DefaultClip;
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
        /// Gets the transform list of the pose at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName">Clip name</param>
        public Matrix[] GetPoseAtTime(float time, string clipName)
        {
            return this.Skeleton.GetPoseAtTime(time, this.Animations[clipName]);
        }
        /// <summary>
        /// Gets final transform collection
        /// </summary>
        /// <returns>Returns final transform collection</returns>
        public Matrix[] GetFinalTransforms()
        {
            return this.Skeleton.FinalTransforms;
        }

        public ShaderResourceView CreateAnimationTexture(Game game)
        {
            List<Vector4> values = new List<Vector4>();

            const float timestep = 1.0f / 30.0f;
            foreach (var clip in this.Clips)
            {
                for (float t = 0; t < this.Duration; t += timestep)
                {
                    var mat = this.GetPoseAtTime(t, clip);

                    for (int m = 0; m < mat.Length; m++)
                    {
                        values.Add(new Vector4(mat[m].Row1.X, mat[m].Row1.Y, mat[m].Row1.Z, mat[m].Row4.X));
                        values.Add(new Vector4(mat[m].Row2.X, mat[m].Row2.Y, mat[m].Row2.Z, mat[m].Row4.Y));
                        values.Add(new Vector4(mat[m].Row3.X, mat[m].Row3.Y, mat[m].Row3.Z, mat[m].Row4.Z));
                    }
                }
            }

            return game.Graphics.Device.CreateTexture(1024, values.ToArray());
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
