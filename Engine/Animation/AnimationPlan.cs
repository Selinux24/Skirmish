using System.Collections.Generic;

namespace Engine.Animation
{
    /// <summary>
    /// Animation plan for animation controller
    /// </summary>
    public class AnimationPlan : List<AnimationPath>
    {
        /// <summary>
        /// Gets the total plan's duration
        /// </summary>
        public float PlannedDuration
        {
            get
            {
                float d = 0;

                for (int i = 0; i < this.Count; i++)
                {
                    d += this[i].TotalDuration;
                }

                return d;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationPlan() : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Animation path</param>
        public AnimationPlan(AnimationPath path) : base(new[] { path })
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public AnimationPlan(IEnumerable<AnimationPath> paths) : base(paths)
        {

        }
    }
}
