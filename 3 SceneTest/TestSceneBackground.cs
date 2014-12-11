using Engine;

namespace SceneTest
{
    public class TestSceneBackground : Scene3D
    {
        private Sprite background = null;

        public TestSceneBackground(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.ContentPath = "ResourcesHID";

            this.background = this.AddSprite(
                "background.jpg",
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight,
                0);
        }
    }
}
