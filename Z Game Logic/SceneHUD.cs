using Engine;
using SharpDX;
using SharpDX.DirectInput;

namespace GameLogic
{
    using GameLogic.Rules;

    public class SceneHUD : Scene3D
    {
        private TextControl txtTitle = null;
        private TextControl txtGame = null;
        private TextControl txtTeam = null;
        private TextControl txtSoldier = null;
        private TextControl txtActionList = null;
        private TextControl txtAction = null;

        private Sprite sprHUD = null;

        public SceneHUD(Game game)
            : base(game)
        {
            this.ContentPath = "Resources";
        }

        public override void Initialize()
        {
            base.Initialize();

            Program.NewGame();

            this.txtTitle = this.AddText("Tahoma", 24, Color.White, Color.Gray);
            this.txtGame = this.AddText("Lucida Casual", 12, Color.LightBlue, Color.DarkBlue);
            this.txtTeam = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtSoldier = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtActionList = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtAction = this.AddText("Lucida Casual", 12, Color.Yellow);

            this.txtTitle.Position = Vector2.Zero;
            this.txtGame.Top = this.txtTitle.Top + this.txtTitle.Height + 1;
            this.txtTeam.Top = this.txtGame.Top + this.txtGame.Height + 1;
            this.txtSoldier.Top = 510;
            this.txtActionList.Top = this.txtSoldier.Top + this.txtSoldier.Height + 1;
            this.txtAction.Top = this.txtActionList.Top + this.txtActionList.Height + 1;

            this.sprHUD = this.AddSprite("HUD.png", 800, 600, 1);

            this.txtTitle.Text = "Game Logic";
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Key.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Key.N))
            {
                Program.NextSoldier(this.Game.Input.KeyPressed(Key.LeftShift));
            }

            if (this.Game.Input.KeyJustReleased(Key.P))
            {
                Program.PrevSoldier(this.Game.Input.KeyPressed(Key.LeftShift));
            }

            if (this.Game.Input.KeyJustReleased(Key.X))
            {
                Program.DoAction();
            }

            if (this.Game.Input.KeyJustReleased(Key.Right))
            {
                Program.NextAction();
            }

            if (this.Game.Input.KeyJustReleased(Key.Left))
            {
                Program.PrevAction();
            }

            if (this.Game.Input.KeyJustReleased(Key.E))
            {
                Victory v = Program.Next();
                if (v != null)
                {
                    this.txtGame.Text = string.Format("{0}", v);
                    this.txtTeam.Text = "";
                    this.txtSoldier.Text = "";
                    this.txtActionList.Text = "";
                    this.txtAction.Text = "";
                }
            }

            if (!Program.GameFinished)
            {
                this.txtGame.Text = string.Format("{0}", Program.SkirmishGame);
                this.txtTeam.Text = string.Format("{0}", Program.SkirmishGame.CurrentTeam);
                this.txtSoldier.Text = string.Format("{0}", Program.SkirmishGame.CurrentSoldier);
                this.txtActionList.Text = string.Format("{0}", Program.CurrentActions.ToStringList());
                this.txtAction.Text = string.Format("{0}", Program.CurrentAction);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
