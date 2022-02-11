using System.Linq;

namespace Engine
{
    /// <summary>
    /// Scene UI extensions
    /// </summary>
    public static class SceneUIExtensions
    {
        /// <summary>
        /// Evaluates input over the specified scene
        /// </summary>
        /// <param name="scene">Scene</param>
        public static void EvaluateInput(this Scene scene)
        {
            scene.TopMostControl = null;

            //Gets all UIControl order by processing order
            var evaluableCtrls = scene.GetComponents<IUIControl>()
                .Where(c => c.IsEvaluable())
                .OrderBy(c => c.GetUpdateOrder())
                .ToList();

            if (!evaluableCtrls.Any())
            {
                return;
            }

            //Initialize state of selected controls
            evaluableCtrls.ForEach(c => c.InitControlState());

            //Gets all controls with the mouse pointer into its bounds
            var mouseOverCtrls = evaluableCtrls.Where(c => c.IsMouseOver);
            if (!mouseOverCtrls.Any())
            {
                return;
            }

            //Reverse the order for processing. Top-most first
            mouseOverCtrls = mouseOverCtrls.Reverse();

            IUIControl focusedControl = null;
            foreach (var topMostControl in mouseOverCtrls)
            {
                //Evaluates all controls with the mouse pointer into its bounds
                topMostControl.EvaluateTopMostControl(out var topControl, out focusedControl);
                if (topControl != null)
                {
                    scene.TopMostControl = topControl;

                    break;
                }
            }

            //Evaluate focused control
            EvaluateFocus(scene, focusedControl);
        }
        /// <summary>
        /// Evaluates the current focus
        /// </summary>
        /// <param name="focusedControl">Current focused control</param>
        /// <remarks>Fires set and lost focus events</remarks>
        public static void EvaluateFocus(this Scene scene, IUIControl focusedControl)
        {
            if (scene.FocusedControl != null)
            {
                var input = scene.Game.Input;

                bool mouseClicked = input.MouseButtonsState != MouseButtons.None;
                bool overFocused = scene.FocusedControl.Contains(input.MousePosition);
                if (mouseClicked && !overFocused)
                {
                    //Clicked outside the current focused control

                    //Lost focus
                    scene.FocusedControl.SetFocusLost();
                    scene.FocusedControl = null;
                }
            }

            if (focusedControl != null && scene.FocusedControl != focusedControl)
            {
                //Clicked on control

                //Set focus
                focusedControl.SetFocusControl();
                scene.FocusedControl = focusedControl;
            }
        }
        /// <summary>
        /// Sets the current focused control
        /// </summary>
        /// <param name="control">Control</param>
        public static void SetFocus(this Scene scene, IUIControl control)
        {
            scene.FocusedControl = control;
        }
        /// <summary>
        /// Clears the current control focus
        /// </summary>
        public static void ClearFocus(this Scene scene)
        {
            scene.FocusedControl = null;
        }
    }
}
