using System;
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
        private SpriteButton butNext = null;
        private SpriteButton butPrevSoldier = null;
        private SpriteButton butNextSoldier = null;
        private SpriteButton butPrevAction = null;
        private SpriteButton butNextAction = null;
        private SpriteButton butDoAction = null;

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

            this.sprHUD = this.AddBackgroud("HUD.png", 99);

            this.butClose = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 60,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 12,
                TextColor = Color.Yellow,
                TextShadowColor = Color.Orange,
                Text = "EXIT",
            });

            this.butNext = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 60,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next",
            });

            this.butPrevSoldier = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Prev.Soldier",
            });

            this.butNextSoldier = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next Soldier",
            });

            this.butPrevAction = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Prev.Action",
            });

            this.butNextAction = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next Action",
            });

            this.butDoAction = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Do Action",
            });

            this.butClose.Click += (sender, eventArgs) => { this.Game.Exit(); };
            this.butNext.Click += (sender, eventArgs) => { this.Next(); };
            this.butPrevSoldier.Click += (sender, eventArgs) => { this.PrevSoldier(); };
            this.butNextSoldier.Click += (sender, eventArgs) => { this.NextSoldier(); };
            this.butPrevAction.Click += (sender, eventArgs) => { this.PrevAction(); };
            this.butNextAction.Click += (sender, eventArgs) => { this.NextAction(); };
            this.butDoAction.Click += (sender, eventArgs) => { this.DoAction(); };

            this.txtTitle.Text = "Game Logic";

            this.UpdateLayout();
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
            Program.NextSoldier(true);
        }
        private void PrevSoldier()
        {
            Program.PrevSoldier(true);
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

        protected override void Resized(object sender, EventArgs e)
        {
            this.UpdateLayout();
        }

        private void UpdateLayout()
        {
            this.txtTitle.Top = 0;
            this.txtTitle.Left = 5;
            this.txtGame.Top = this.txtTitle.Top + this.txtTitle.Height + 1;
            this.txtGame.Left = 10;
            this.txtTeam.Top = this.txtGame.Top + this.txtGame.Height + 1;
            this.txtTeam.Left = this.txtGame.Left;

            this.butClose.Top = 1;
            this.butClose.Left = this.Game.Form.RenderWidth - 60 - 1;

            this.butNext.Top = (int)((float)this.Game.Form.RenderHeight * 0.85f);
            this.butNext.Left = 10;
            this.butPrevSoldier.Top = this.butNext.Top;
            this.butPrevSoldier.Left = this.butNext.Left + this.butNext.Width + 25;
            this.butNextSoldier.Top = this.butNext.Top;
            this.butNextSoldier.Left = this.butPrevSoldier.Left + this.butPrevSoldier.Width + 10;
            this.butPrevAction.Top = this.butNext.Top;
            this.butPrevAction.Left = this.butNextSoldier.Left + this.butNextSoldier.Width + 25;
            this.butNextAction.Top = this.butNext.Top;
            this.butNextAction.Left = this.butPrevAction.Left + this.butPrevAction.Width + 10;
            this.butDoAction.Top = this.butNext.Top;
            this.butDoAction.Left = this.butNextAction.Left + this.butNextAction.Width + 25;

            this.txtSoldier.Top = this.butNext.Top + this.butNext.Height + 1;
            this.txtSoldier.Left = 10;
            this.txtActionList.Top = this.txtSoldier.Top + this.txtSoldier.Height + 1;
            this.txtActionList.Left = this.txtSoldier.Left;
            this.txtAction.Top = this.txtActionList.Top + this.txtActionList.Height + 1;
            this.txtAction.Left = this.txtSoldier.Left;
        }
    }
}
