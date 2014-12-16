using Engine;
using SharpDX.DirectInput;
using SharpDX;

namespace GameLogic
{
    using GameLogic.Rules;

    public class TestScene3D : Scene3D
    {
        private TextControl title = null;
        private TextControl skirmish = null;
        private TextControl soldier = null;
        private TextControl info = null;
        private Skirmish skGame = null;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.skGame = new Skirmish();
            this.skGame.AddTeam("Team1", "Reds", TeamRole.Defense, 8);
            this.skGame.AddTeam("Team2", "Blues", TeamRole.Assault, 10);
            this.skGame.AddTeam("Team3", "Gray", TeamRole.Neutral, 4);
            this.skGame.Start();

            this.title = this.AddText("Tahoma", 24, Color.White);
            this.title.Text = "Game Logic";
            this.title.Position = Vector2.Zero;

            this.skirmish = this.AddText("Lucida Casual", 12, Color.LightBlue);
            this.soldier = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.info = this.AddText("Lucida Casual", 12, Color.Yellow);

            this.skirmish.Top = this.title.Top + this.title.Height + 1;
            this.soldier.Top = this.skirmish.Top + this.skirmish.Height + 1;
            this.info.Top = this.soldier.Top + this.soldier.Height + 1;
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
                this.skGame.NextSoldier(this.Game.Input.KeyPressed(Key.LeftShift));
            }

            if (this.Game.Input.KeyJustReleased(Key.P))
            {
                this.skGame.PrevSoldier(this.Game.Input.KeyPressed(Key.LeftShift));
            }

            if (this.Game.Input.KeyJustReleased(Key.A))
            {
                SoldierAction[] actions = this.skGame.GetActions();
                if (actions.Length > 0)
                {
                    SoldierAction action = actions[0];

                    action.Active = this.skGame.CurrentSoldier;
                    action.Passive = this.skGame.CurrentSoldier;

                    this.skGame.DoAction(action);
                }
            }

            if (this.Game.Input.KeyJustReleased(Key.E))
            {
                this.skGame.NextPhase();
            }

            this.skirmish.Text = string.Format("{0} | {1}", this.skGame, this.skGame.CurrentTeam);
            this.soldier.Text = string.Format("{0}", this.skGame.CurrentSoldier);
            this.info.Text = "";
        }
    }
}
