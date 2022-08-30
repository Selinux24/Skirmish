using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.Common;

    public struct BuiltInDrawerMeshState
    {
        public Matrix Local { get; set; }
        public AnimationDrawInfo Animation { get; set; }
    }

    public struct BuiltInDrawerMaterialState
    {
        public MaterialDrawInfo Material { get; set; }
        public Color4 TintColor { get; set; }
        public uint TextureIndex { get; set; }
    }
}
