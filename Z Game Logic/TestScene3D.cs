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
        private TextControl actions = null;
        private TextControl info = null;
        
        private Skirmish skGame = null;
        private int actionIndex = 0;
        private Actions[] currentActions = null;
        private Actions CurrentAction
        {
            get
            {
                if (this.currentActions != null && this.currentActions.Length > 0)
                {
                    return this.currentActions[this.actionIndex];
                }

                return null;
            }
        }

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.skGame = new Skirmish();
            this.skGame.AddTeam("Team1", "Reds", TeamRole.Defense, 5);
            this.skGame.AddTeam("Team2", "Blues", TeamRole.Assault, 5);
            this.skGame.AddTeam("Team3", "Gray", TeamRole.Neutral, 2);
            this.skGame.Start();
            this.currentActions = this.skGame.GetActions();

            this.title = this.AddText("Tahoma", 24, Color.White);
            this.title.Text = "Game Logic";
            this.title.Position = Vector2.Zero;

            this.skirmish = this.AddText("Lucida Casual", 12, Color.LightBlue);
            this.soldier = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.actions = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.info = this.AddText("Lucida Casual", 12, Color.Yellow);

            this.skirmish.Top = this.title.Top + this.title.Height + 1;
            this.soldier.Top = this.skirmish.Top + this.skirmish.Height + 1;
            this.actions.Top = this.soldier.Top + this.soldier.Height + 1;
            this.info.Top = this.actions.Top + this.actions.Height + 1;
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
                Actions action = this.CurrentAction;
                if (action != null)
                {
                    if (action is ActionsMovement)
                    {
                        ((ActionsMovement)action).Active = this.skGame.CurrentSoldier;
                        ((ActionsMovement)action).Distance = 10;

                        if (action is Assault)
                        {
                            Soldier enemy = this.GetRandomEnemy();
                         
                            this.skGame.JoinMelee(this.skGame.CurrentSoldier, enemy);
                        }
                    }
                    else if (action is ActionsShooting)
                    {
                        ((ActionsShooting)action).Active = this.skGame.CurrentSoldier;

                        if (action is Shoot)
                        {
                            ((Shoot)action).Passive = this.GetRandomEnemy();
                        }
                        else if (action is FirstAid)
                        {
                            ((FirstAid)action).Passive = this.GetRandomFriend();
                        }
                    }
                    else if (action is ActionsMelee)
                    {
                        ((ActionsMelee)action).Active = this.skGame.CurrentSoldier;

                        if (action is Fight || action is Leave)
                        {
                            ((ActionsMelee)action).Melee = this.skGame.GetMelee(this.skGame.CurrentSoldier);
                        }
                    }
                    else if (action is ActionsMorale)
                    {
                        ((ActionsMorale)action).Active = this.skGame.CurrentSoldier;
                    }

                    this.skGame.DoAction(action);
                }
            }

            if (this.Game.Input.KeyJustReleased(Key.E))
            {
                this.skGame.Next();

                this.currentActions = this.skGame.GetActions();
                this.actionIndex = 0;
            }

            if (this.Game.Input.KeyJustReleased(Key.Right))
            {
                this.NextAction();
            }

            if (this.Game.Input.KeyJustReleased(Key.Left))
            {
                this.PrevAction();
            }

            this.skirmish.Text = string.Format("{0} | {1}", this.skGame, this.skGame.CurrentTeam);
            this.soldier.Text = string.Format("{0}", this.skGame.CurrentSoldier);
            this.actions.Text = string.Format("{0}", this.skGame.GetActions().ToStringList());
            this.info.Text = string.Format("{0}", this.CurrentAction);
        }

        private Soldier GetRandomEnemy()
        {
            Team[] enemyTeams = this.skGame.EnemyOf(this.skGame.CurrentTeam);
            if (enemyTeams.Length > 0)
            {
                return enemyTeams[0].Soldiers[0];
            }

            return null;
        }
        private Soldier GetRandomFriend()
        {
            Team[] enemyTeams = this.skGame.FriendOf(this.skGame.CurrentTeam);
            if (enemyTeams.Length > 0)
            {
                return enemyTeams[0].Soldiers[0];
            }

            return null;
        }
        private void NextAction()
        {
            if (this.currentActions != null && this.currentActions.Length > 0)
            {
                this.actionIndex++;

                if (this.actionIndex > this.currentActions.Length - 1)
                {
                    this.actionIndex = 0;
                }
            }
        }
        private void PrevAction()
        {
            if (this.currentActions != null && this.currentActions.Length > 0)
            {
                this.actionIndex--;

                if (this.actionIndex < 0)
                {
                    this.actionIndex = this.currentActions.Length - 1;
                }
            }
        }
    }
}
