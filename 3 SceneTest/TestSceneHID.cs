using Engine;
using SharpDX;

namespace SceneTest
{
    public class TestSceneHID : Scene3D
    {
        private TextControl title = null;
        private Sprite sprite = null;

        public TestSceneHID(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.ContentPath = "ResourcesHID";

            this.title = this.AddText("Tahoma", 18, Color.BlueViolet, 0);
            this.title.Text = "3D scene & HID scene";
            this.title.Position = Vector2.Zero;

            this.sprite = this.AddSprite(
                "smiley.jpg",
                128,
                128,
                1);
        }
    }
}
