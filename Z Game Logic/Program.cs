using System;
using Engine;
using SharpDX;

namespace GameLogic
{
    using GameLogic.Rules;

    static class Program
    {
        private static Game game = null;
        private static int actionIndex = 0;

        private static SceneHUD hudScene = null;
        private static SceneObjects objectsScene = null;

        public static Actions[] CurrentActions = null;
        public static Skirmish SkirmishGame = null;
        public static bool GameFinished = false;
        public static Actions CurrentAction
        {
            get
            {
                if (CurrentActions != null && CurrentActions.Length > 0)
                {
                    return CurrentActions[actionIndex];
                }

                return null;
            }
        }

        static void Main()
        {
#if DEBUG
            using (game = new Game("Game Logic", false, 800, 600))
#else
            using (game = new Game("Game Logic", false, 800, 600))
#endif
            {
                game.VisibleMouse = true;
                game.LockMouse = false;

                GameEnvironment.Background = Color.CornflowerBlue;

                hudScene = new SceneHUD(game) { Active = true, Order = 1 };

                game.AddScene(hudScene);

                game.Run();
            }
        }

        public static void NewGame()
        {
            Program.SkirmishGame = new Skirmish();
            Program.SkirmishGame.AddTeam("Team1", "Reds", TeamRole.Defense, 5, 1, 1);
            Program.SkirmishGame.AddTeam("Team2", "Blues", TeamRole.Assault, 5, 1, 1);
            //Program.SkirmishGame.AddTeam("Team3", "Gray", TeamRole.Neutral, 2, 0, 0, false);
            Program.SkirmishGame.Start();
            Program.CurrentActions = Program.SkirmishGame.GetActions();

            objectsScene = new SceneObjects(game) { Active = true, Order = game.SceneCount };

            game.AddScene(objectsScene);
        }
        public static void NextSoldier(bool selectIdle)
        {
            Program.SkirmishGame.NextSoldier(selectIdle);

            Program.CurrentActions = Program.SkirmishGame.GetActions();

            Program.objectsScene.GoToSoldier(Program.SkirmishGame.CurrentSoldier);
        }
        public static void PrevSoldier(bool selectIdle)
        {
            Program.SkirmishGame.PrevSoldier(selectIdle);

            Program.CurrentActions = Program.SkirmishGame.GetActions();

            Program.objectsScene.GoToSoldier(Program.SkirmishGame.CurrentSoldier);
        }
        public static void DoAction()
        {
            Actions action = Program.CurrentAction;
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

                Program.SkirmishGame.DoAction(action);

                Program.CurrentActions = Program.SkirmishGame.GetActions();
            }
        }
        public static Victory Next()
        {
            Program.SkirmishGame.Next();

            Program.CurrentActions = Program.SkirmishGame.GetActions();
            Program.actionIndex = 0;

            Victory v = Program.SkirmishGame.IsFinished();
            if (v != null)
            {
                Program.GameFinished = true;
            }

            return v;
        }

        private static void MovementActions(ActionsMovement action)
        {
            if (action is Move || action is Run || action is Crawl)
            {
                //Point to point
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.WastedPoints = Program.SkirmishGame.CurrentSoldier.CurrentMovingCapacity;
            }
            else if (action is Assault)
            {
                //Point to enemy
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.Passive = Program.GetRandomEnemy();
                action.WastedPoints = Program.SkirmishGame.CurrentSoldier.CurrentMovingCapacity;

                Program.SkirmishGame.JoinMelee(action.Active, action.Passive);
            }
            else if (action is CoveringFire)
            {
                //Area
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.Area = new Area();
            }
            else if (action is Reload || action is Repair || action is Inventory || action is UseMovementItem)
            {
                //None
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.WastedPoints = Program.SkirmishGame.CurrentSoldier.CurrentMovingCapacity;
            }
            else if (action is Communications)
            {
                //None
                action.Active = Program.SkirmishGame.CurrentSoldier;
            }
            else if (action is FindCover || action is RunAway)
            {
                //Point to point
                action.Active = Program.SkirmishGame.CurrentSoldier;
            }
        }
        private static void ShootingActions(ActionsShooting action)
        {
            if (action is Shoot)
            {
                //Needs target
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.Passive = Program.GetRandomEnemy();
                action.WastedPoints = Program.SkirmishGame.CurrentSoldier.CurrentActionPoints;
            }
            else if (action is SupressingFire)
            {
                //Needs area
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.Area = new Area();
            }
            else if (action is Support)
            {
                //None
                action.Active = Program.SkirmishGame.CurrentSoldier;
            }
            else if (action is UseShootingItem)
            {
                //None
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.WastedPoints = Program.SkirmishGame.CurrentSoldier.CurrentActionPoints;
            }
            else if (action is FirstAid)
            {
                //Needs target
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.Passive = Program.GetRandomFriend();
                action.WastedPoints = Program.SkirmishGame.CurrentSoldier.CurrentActionPoints;
            }
        }
        private static void MeleeActions(ActionsMelee action)
        {
            if (action is Leave)
            {
                //None
                action.Active = Program.SkirmishGame.CurrentSoldier;
                action.Melee = Program.SkirmishGame.GetMelee(Program.SkirmishGame.CurrentSoldier);
            }
            else if (action is UseMeleeItem)
            {
                //None
                action.Active = Program.SkirmishGame.CurrentSoldier;
            }
        }
        private static void MoraleActions(ActionsMorale action)
        {
            if (action is UseMoraleItem)
            {
                //None
                action.Active = Program.SkirmishGame.CurrentSoldier;
            }
        }

        public static Soldier GetRandomEnemy()
        {
            Random rnd = new Random();

            Team[] enemyTeams = Program.SkirmishGame.EnemyOf(Program.SkirmishGame.CurrentTeam);
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
        public static Soldier GetRandomFriend()
        {
            Random rnd = new Random();

            Team[] friendlyTeams = Program.SkirmishGame.FriendOf(Program.SkirmishGame.CurrentTeam);
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
        public static void NextAction()
        {
            if (Program.CurrentActions != null && Program.CurrentActions.Length > 0)
            {
                Program.actionIndex++;

                if (Program.actionIndex > Program.CurrentActions.Length - 1)
                {
                    Program.actionIndex = 0;
                }
            }
        }
        public static void PrevAction()
        {
            if (Program.CurrentActions != null && Program.CurrentActions.Length > 0)
            {
                Program.actionIndex--;

                if (Program.actionIndex < 0)
                {
                    Program.actionIndex = Program.CurrentActions.Length - 1;
                }
            }
        }
    }
}
