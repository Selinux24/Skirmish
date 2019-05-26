using Engine.Animation;
using System.Collections.Generic;

namespace Engine
{
    public class ModularSceneryAction
    {
        class ControllerAction
        {
            public AnimationController Controller { get; set; }
            public AnimationPlan Action { get; set; }

            public void Start()
            {
                Controller?.SetPath(Action);
                Controller?.Start();
            }

            public void Pause()
            {
                Controller?.Pause();
            }
        }

        private readonly List<ControllerAction> controllers = new List<ControllerAction>();

        public ModularSceneryAction()
        {

        }

        public void AddController(AnimationController controller, AnimationPlan animation)
        {
            var curr = controllers.Find(c => c.Controller == controller);
            if (curr != null)
            {
                curr.Action = animation;
            }
            else
            {
                controllers.Add(new ControllerAction()
                {
                    Controller = controller,
                    Action = animation,
                });
            }
        }

        public void Start()
        {
            controllers.ForEach(c => c.Start());
        }

        public void Pause()
        {
            controllers.ForEach(c => c.Pause());
        }
    }
}
