using Engine;
using SharpDX;

namespace SceneTest
{
    public class TestSceneHID : Scene3D
    {
        private Sprite sprite = null;

        private TextDrawer title = null;
        private TextDrawer txt0 = null;
        private TextDrawer txt1 = null;
        private TextDrawer txt2 = null;
        private TextDrawer txt3 = null;
        private TextDrawer txt4 = null;
        private TextDrawer txt5 = null;
        private TextDrawer txt6 = null;
        private TextDrawer txt7 = null;
        private TextDrawer txt8 = null;
        private TextDrawer txt9 = null;

        private int currentSize = 10;

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

            SpriteDescription spriteDesc = new SpriteDescription()
            {
                Textures = new[] { "smiley.jpg" },
                Width = 128,
                Height = 128,
                FitScreen = true,
            };
            this.sprite = this.AddSprite(spriteDesc, 1);

            this.InitializeText(this.currentSize);
        }

        private void InitializeText(int size)
        {
            this.txt0 = this.AddText("Times New Roman", size + 0, Color.Red, 0);
            this.txt1 = this.AddText("Times New Roman", size + 1, Color.Red, 0);
            this.txt2 = this.AddText("Times New Roman", size + 2, Color.Red, 0);
            this.txt3 = this.AddText("Times New Roman", size + 3, Color.Red, 0);
            this.txt4 = this.AddText("Times New Roman", size + 4, Color.Red, 0);
            this.txt5 = this.AddText("Times New Roman", size + 5, Color.Red, 0);
            this.txt6 = this.AddText("Times New Roman", size + 6, Color.Red, 0);
            this.txt7 = this.AddText("Times New Roman", size + 7, Color.Red, 0);
            this.txt8 = this.AddText("Times New Roman", size + 8, Color.Red, 0);
            this.txt9 = this.AddText("Times New Roman", size + 9, Color.Red, 0);

            this.txt0.Text = this.txt0.Font;
            this.txt1.Text = this.txt1.Font;
            this.txt2.Text = this.txt2.Font;
            this.txt3.Text = this.txt3.Font;
            this.txt4.Text = this.txt4.Font;
            this.txt5.Text = this.txt5.Font;
            this.txt6.Text = this.txt6.Font;
            this.txt7.Text = this.txt7.Font;
            this.txt8.Text = this.txt8.Font;
            this.txt9.Text = this.txt9.Font;

            this.txt0.Top = this.title.Top + this.title.Height + 1;
            this.txt1.Top = this.txt0.Top + this.txt0.Height + 1;
            this.txt2.Top = this.txt1.Top + this.txt1.Height + 1;
            this.txt3.Top = this.txt2.Top + this.txt2.Height + 1;
            this.txt4.Top = this.txt3.Top + this.txt3.Height + 1;
            this.txt5.Top = this.txt4.Top + this.txt4.Height + 1;
            this.txt6.Top = this.txt5.Top + this.txt5.Height + 1;
            this.txt7.Top = this.txt6.Top + this.txt6.Height + 1;
            this.txt8.Top = this.txt7.Top + this.txt7.Height + 1;
            this.txt9.Top = this.txt8.Top + this.txt8.Height + 1;
        }
        private void ReleaseText()
        {
            this.RemoveComponent(this.txt0);
            this.RemoveComponent(this.txt1);
            this.RemoveComponent(this.txt2);
            this.RemoveComponent(this.txt3);
            this.RemoveComponent(this.txt4);
            this.RemoveComponent(this.txt5);
            this.RemoveComponent(this.txt6);
            this.RemoveComponent(this.txt7);
            this.RemoveComponent(this.txt8);
            this.RemoveComponent(this.txt9);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                this.currentSize++;

                this.ReleaseText();
                this.InitializeText(this.currentSize);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Subtract))
            {
                this.currentSize--;

                if (this.currentSize <= 0) this.currentSize = 1;

                this.ReleaseText();
                this.InitializeText(this.currentSize);
            }

            this.Game.Form.Text = string.Format("Rel {0} - Abs {1}", this.Game.Form.RelativeCenter, this.Game.Form.AbsoluteCenter);
        }
    }
}
