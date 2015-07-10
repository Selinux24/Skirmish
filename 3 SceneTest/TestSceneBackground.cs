using Engine;

namespace SceneTest
{
    public class TestSceneBackground : Scene
    {
        private Sprite background = null;

        public TestSceneBackground(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            BackgroundDescription bkDesc = new BackgroundDescription()
            {
                ContentPath = "ResourcesHID",
                Textures = new[] { "background.jpg" },
            };

            this.background = this.AddBackgroud(bkDesc);
        }
    }
}
