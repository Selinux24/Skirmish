﻿using AISamples.Common.Persistence;
using Engine;
using Engine.BuiltIn.Format;
using SharpDX;

namespace AISamples.Common.Markings
{
    class Target(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        public override IMarkingFile FromMarking()
        {
            return new TargetFile
            {
                Type = nameof(Target),
                Position = Vector2File.FromVector2(Position),
                Direction = Vector2File.FromVector2(Direction),
                Width = Width,
                Height = Height,
                Is3D = Is3D
            };
        }

        protected override VertexPositionTexture[] CreateMarking(float height)
        {
            var support = GetSupport();
            var uvs = Constants.TargetUVs;

            return CreateQuadFromSupport(Width, height, support, uvs, 1f);
        }

        public override bool Update(IGameTime gameTime)
        {
            return false;
        }
    }
}
