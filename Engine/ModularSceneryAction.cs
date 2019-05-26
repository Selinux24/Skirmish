using System.Collections.Generic;

namespace Engine
{
    using Engine.Animation;

    /// <summary>
    /// Modular scenery action
    /// </summary>
    public class ModularSceneryAction
    {
        /// <summary>
        /// Controller action
        /// </summary>
        public class ControllerAction
        {
            /// <summary>
            /// Instance
            /// </summary>
            private ModelInstance instance = null;
            /// <summary>
            /// Current animation plan
            /// </summary>
            private AnimationPlan currentPlan = null;

            /// <summary>
            /// Instance Id
            /// </summary>
            public string InstanceId { get; set; }
            /// <summary>
            /// Animation plant name
            /// </summary>
            public string AnimationPlanName { get; set; }

            /// <summary>
            /// Initializes the controller action
            /// </summary>
            /// <param name="instance">Instance</param>
            /// <param name="plan">Animation plan</param>
            public void Initialize(ModelInstance instance, AnimationPlan plan)
            {
                this.instance = instance;
                this.currentPlan = plan;
            }
            /// <summary>
            /// Starts the controller
            /// </summary>
            public void Start()
            {
                if (currentPlan == null || instance?.AnimationController == null)
                {
                    return;
                }

                instance.AnimationController.SetPath(currentPlan);
                instance.AnimationController.Start();
            }
            /// <summary>
            /// Pauses the controller
            /// </summary>
            public void Pause()
            {
                if (instance?.AnimationController == null)
                {
                    return;
                }

                instance.AnimationController.Pause();
            }
        }

        /// <summary>
        /// Controllers list
        /// </summary>
        public List<ControllerAction> Controllers { get; set; } = new List<ControllerAction>();

        /// <summary>
        /// Constructor
        /// </summary>
        public ModularSceneryAction()
        {

        }

        /// <summary>
        /// Adds a new instance animation to the action
        /// </summary>
        /// <param name="instanceId">Instance id</param>
        /// <param name="animationPlanName">Animation plan name</param>
        public void Add(string instanceId, string animationPlanName)
        {
            var curr = Controllers.Find(c => c.InstanceId == instanceId);
            if (curr != null)
            {
                curr.AnimationPlanName = animationPlanName;
            }
            else
            {
                Controllers.Add(new ControllerAction()
                {
                    InstanceId = instanceId,
                    AnimationPlanName = animationPlanName,
                });
            }
        }
        /// <summary>
        /// Starts the action
        /// </summary>
        public void Start()
        {
            Controllers.ForEach(c => c.Start());
        }
        /// <summary>
        /// Pauses de action
        /// </summary>
        public void Pause()
        {
            Controllers.ForEach(c => c.Pause());
        }
    }
}
