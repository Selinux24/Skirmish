using SharpDX;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Common
{
    using Common.Utils;

    public class Sprite : Drawable
    {
        protected Scene3D Scene { get; private set; }
        protected Matrix LocalTransform = Matrix.Identity;
        protected Vector2 Position = Vector2.Zero;
        protected EffectBase Effect = null;
        protected Geometry Geometry = null;
        protected ShaderResourceView Texture = null;

        public Sprite(Game game, Scene3D scene)
            : base(game)
        {
            this.Scene = scene;
        }
        public override void Dispose()
        {
            if (this.Texture != null)
            {
                this.Texture.Dispose();
                this.Texture = null;
            }

            if (this.Geometry != null)
            {
                this.Geometry.Dispose();
                this.Geometry = null;
            }

            if (this.Effect != null)
            {
                this.Effect.Dispose();
                this.Effect = null;
            }

            base.Dispose();
        }
        public override void Update()
        {
            base.Update();

            this.LocalTransform = Matrix.Translation(this.Position.X, this.Position.Y, 0f);
        }

        public virtual void Move(Vector2 d)
        {
            this.Position += d;
        }
        public virtual void MoveLeft(float d)
        {
            this.Position += Vector2.UnitX * -d;
        }
        public virtual void MoveRight(float d)
        {
            this.Position += Vector2.UnitX * d;
        }
        public virtual void MoveUp(float d)
        {
            this.Position += Vector2.UnitY * d;
        }
        public virtual void MoveDown(float d)
        {
            this.Position += Vector2.UnitY * -d;
        }

        public virtual void SetPosition(float x, float y)
        {
            this.SetPosition(new Vector2(x, y));
        }
        public virtual void SetPosition(Vector2 position)
        {
            this.Position = position;
        }
    }
}
