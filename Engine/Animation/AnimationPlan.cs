using System.Collections.Generic;
using System.Linq;

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
                return this.Sum(i => i.TotalDuration);
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
        public AnimationPlan(params AnimationPath[] paths) : base(paths)
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
