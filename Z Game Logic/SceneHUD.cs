using Engine;
using SharpDX;

namespace GameLogic
{
    using GameLogic.Rules;

    public class SceneHUD : Scene3D
    {
        private TextDrawer txtTitle = null;
        private TextDrawer txtGame = null;
        private TextDrawer txtTeam = null;
        private TextDrawer txtSoldier = null;
        private TextDrawer txtActionList = null;
        private TextDrawer txtAction = null;

        private Sprite sprHUD = null;
        private SpriteButton butClose = null;
        private SpriteButton butNextSoldier = null;
        private SpriteButton butPrevSoldier = null;
        private SpriteButton butNextAction = null;
        private SpriteButton butPrevAction = null;
        private SpriteButton butDoAction = null;
        private SpriteButton butNext = null;

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
            this.txtSoldier.Top = 540;
            this.txtActionList.Top = this.txtSoldier.Top + this.txtSoldier.Height + 1;
            this.txtAction.Top = this.txtActionList.Top + this.txtActionList.Height + 1;

            this.sprHUD = this.AddSprite("HUD.png", 800, 600, 99);

            this.butClose = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Left = 800 - 60 - 10,
                Top = 10,
                Width = 60,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 12,
                TextColor = Color.Yellow,
                TextShadowColor = Color.Orange,
                Text = "EXIT",
            });
            this.butClose.Click += (sender, eventArgs) => { this.Game.Exit(); };

            this.butNext = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Left = 10,
                Top = 510,
                Width = 60,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next",
            });
            this.butNext.Click += (sender, eventArgs) => { this.Next(); };

            this.butNextSoldier = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Left = this.butNext.Left + this.butNext.Width + 10,
                Top = 510,
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next Soldier",
            });
            this.butNextSoldier.Click += (sender, eventArgs) => { this.NextSoldier(); };

            this.butPrevSoldier = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Left = this.butNextSoldier.Left + this.butNextSoldier.Width + 10,
                Top = 510,
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Prev.Soldier",
            });
            this.butPrevSoldier.Click += (sender, eventArgs) => { this.PrevSoldier(); };

            this.butNextAction = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Left = this.butPrevSoldier.Left + this.butPrevSoldier.Width + 10,
                Top = 510,
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next Action",
            });
            this.butNextAction.Click += (sender, eventArgs) => { this.NextAction(); };

            this.butPrevAction = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Left = this.butNextAction.Left + this.butNextAction.Width + 10,
                Top = 510,
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Prev.Action",
            });
            this.butPrevAction.Click += (sender, eventArgs) => { this.PrevAction(); };

            this.butDoAction = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Left = this.butPrevAction.Left + this.butPrevAction.Width + 10,
                Top = 510,
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Do Action",
            });
            this.butDoAction.Click += (sender, eventArgs) => { this.DoAction(); };

            this.txtTitle.Text = "Game Logic";
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.N))
            {
                this.NextSoldier();
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.PrevSoldier();
            }

            if (this.Game.Input.KeyJustReleased(Keys.X))
            {
                this.DoAction();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.NextAction();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.PrevAction();
            }

            if (this.Game.Input.KeyJustReleased(Keys.E))
            {
                this.Next();
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

        private void Next()
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
        private void NextSoldier()
        {
            Program.NextSoldier(this.Game.Input.KeyPressed(Keys.LShiftKey));
        }
        private void PrevSoldier()
        {
            Program.PrevSoldier(this.Game.Input.KeyPressed(Keys.LShiftKey));
        }
        private void NextAction()
        {
            Program.NextAction();
        }
        private void PrevAction()
        {
            Program.PrevAction();
        }
        private void DoAction()
        {
            Program.DoAction();
        }
    }
}
