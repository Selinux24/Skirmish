using System.Threading.Tasks;

namespace Engine.UI
{
    /// <summary>
    /// User interface panel
    /// </summary>
    public class UIPanel : UIControl
    {
        private readonly Sprite background;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UIPanel(Scene scene, UIPanelDescription description) : base(scene, description)
        {
            if (description.Background != null)
            {
                background = new Sprite(scene, description.Background)
                {
                    Name = $"{description.Name}.Background",
                    FitParent = true,
                };

                this.AddChild(background);
            }
        }
    }

    /// <summary>
    /// Sprite button extensions
    /// </summary>
    public static class UIPanelExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIPanel> AddComponentUIPanel(this Scene scene, UIPanelDescription description, int order = 0)
        {
            UIPanel component = null;

            await Task.Run(() =>
            {
                component = new UIPanel(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
