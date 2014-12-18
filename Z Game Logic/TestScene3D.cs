using System;
using Engine;
using SharpDX;
using SharpDX.DirectInput;

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
        private bool gameFinished = false;
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
            //this.skGame.AddTeam("Team3", "Gray", TeamRole.Neutral, 2);
            this.skGame.Start();
            this.currentActions = this.skGame.GetActions();

            this.title = this.AddText("Tahoma", 24, Color.White, Color.Gray);
            this.title.Text = "Game Logic";
            this.title.Position = Vector2.Zero;

            this.skirmish = this.AddText("Lucida Casual", 12, Color.LightBlue, Color.DarkBlue);
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

                this.currentActions = this.skGame.GetActions();
            }

            if (this.Game.Input.KeyJustReleased(Key.P))
            {
                this.skGame.PrevSoldier(this.Game.Input.KeyPressed(Key.LeftShift));

                this.currentActions = this.skGame.GetActions();
            }

            if (this.Game.Input.KeyJustReleased(Key.A))
            {
                Actions action = this.CurrentAction;
                if (action != null)
                {
                    if (action is ActionsMovement)
                    {
                        MovementActions((ActionsMovement)action);
                    }
                    else if (action is ActionsShooting)
                    {
                        ShootingActions((ActionsShooting)action);
                    }
                    else if (action is ActionsMelee)
                    {
                        MeleeActions((ActionsMelee)action);
                    }
                    else if (action is ActionsMorale)
                    {
                        MoraleActions((ActionsMorale)action);
                    }

                    this.skGame.DoAction(action);

                    this.currentActions = this.skGame.GetActions();
                }
            }

            if (this.Game.Input.KeyJustReleased(Key.E))
            {
                this.skGame.Next();

                this.currentActions = this.skGame.GetActions();
                this.actionIndex = 0;

                Victory v = this.skGame.IsFinished();
                if (v != null)
                {
                    this.gameFinished = true;

                    this.skirmish.Text = string.Format("{0}", v);
                    this.soldier.Text = "";
                    this.actions.Text = "";
                    this.info.Text = "";
                }
            }

            if (this.Game.Input.KeyJustReleased(Key.Right))
            {
                this.NextAction();
            }

            if (this.Game.Input.KeyJustReleased(Key.Left))
            {
                this.PrevAction();
            }

            if (!this.gameFinished)
            {
                this.skirmish.Text = string.Format("{0} | {1}", this.skGame, this.skGame.CurrentTeam);
                this.soldier.Text = string.Format("{0}", this.skGame.CurrentSoldier);
                this.actions.Text = string.Format("{0}", this.currentActions.ToStringList());
                this.info.Text = string.Format("{0}", this.CurrentAction);
            }
        }

        private void MovementActions(ActionsMovement action)
        {
            if (action is Move || action is Run || action is Crawl)
            {
                //Point to point
                action.Active = this.skGame.CurrentSoldier;
                action.WastedPoints = this.skGame.CurrentSoldier.CurrentMovingCapacity;
            }
            else if (action is Assault)
            {
                //Point to enemy
                action.Active = this.skGame.CurrentSoldier;
                action.Passive = this.GetRandomEnemy();
                action.WastedPoints = this.skGame.CurrentSoldier.CurrentMovingCapacity;

                this.skGame.JoinMelee(action.Active, action.Passive);
            }
            else if (action is CoveringFire)
            {
                //Area
                action.Active = this.skGame.CurrentSoldier;
                action.Area = new Area();
            }
            else if (action is Reload || action is Repair || action is Inventory || action is UseMovementItem)
            {
                //None
                action.Active = this.skGame.CurrentSoldier;
                action.WastedPoints = this.skGame.CurrentSoldier.CurrentMovingCapacity;
            }
            else if (action is Communications)
            {
                //None
                action.Active = this.skGame.CurrentSoldier;
            }
            else if (action is FindCover || action is RunAway)
            {
                //Point to point
                action.Active = this.skGame.CurrentSoldier;
            }
        }
        private void ShootingActions(ActionsShooting action)
        {
            if (action is Shoot)
            {
                //Needs target
                action.Active = this.skGame.CurrentSoldier;
                action.Passive = this.GetRandomEnemy();
                action.WastedPoints = this.skGame.CurrentSoldier.CurrentActionPoints;
            }
            else if (action is SupressingFire)
            {
                //Needs area
                action.Active = this.skGame.CurrentSoldier;
                action.Area = new Area();
            }
            else if (action is Support)
            {
                //None
                action.Active = this.skGame.CurrentSoldier;
            }
            else if (action is UseShootingItem)
            {
                //None
                action.Active = this.skGame.CurrentSoldier;
                action.WastedPoints = this.skGame.CurrentSoldier.CurrentActionPoints;
            }
            else if (action is FirstAid)
            {
                //Needs target
                action.Active = this.skGame.CurrentSoldier;
                action.Passive = this.GetRandomFriend();
                action.WastedPoints = this.skGame.CurrentSoldier.CurrentActionPoints;
            }
        }
        private void MeleeActions(ActionsMelee action)
        {
            if (action is Leave)
            {
                //None
                action.Active = this.skGame.CurrentSoldier;
                action.Melee = this.skGame.GetMelee(this.skGame.CurrentSoldier);
            }
            else if (action is UseMeleeItem)
            {
                //None
                action.Active = this.skGame.CurrentSoldier;
            }
        }
        private void MoraleActions(ActionsMorale action)
        {
            if (action is UseMoraleItem)
            {
                //None
                action.Active = this.skGame.CurrentSoldier;
            }
        }

        private Soldier GetRandomEnemy()
        {
            Random rnd = new Random();

            Team[] enemyTeams = this.skGame.EnemyOf(this.skGame.CurrentTeam);
            if (enemyTeams.Length > 0)
            {
                int t = rnd.Next(enemyTeams.Length - 1);

                Team team = enemyTeams[t];

                Soldier[] aliveSoldiers = Array.FindAll(team.Soldiers, w => w.CurrentHealth != HealthStates.Disabled);

                int s = rnd.Next(aliveSoldiers.Length - 1);

                Soldier soldier = aliveSoldiers[s];

                return soldier;
            }

            return null;
        }
        private Soldier GetRandomFriend()
        {
            Random rnd = new Random();

            Team[] friendlyTeams = this.skGame.FriendOf(this.skGame.CurrentTeam);
            if (friendlyTeams.Length > 0)
            {
                int t = rnd.Next(friendlyTeams.Length - 1);

                Team team = friendlyTeams[t];

                Soldier[] woundedSoldiers = Array.FindAll(team.Soldiers, w => w.CurrentHealth != HealthStates.Healthy);

                int s = rnd.Next(woundedSoldiers.Length - 1);

                Soldier soldier = woundedSoldiers[s];

                return soldier;
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
