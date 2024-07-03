
namespace TerrainSamples.SceneGrid.Rules
{
    using TerrainSamples.SceneGrid.Rules.Enum;

    public abstract class Item(string name, ItemClasses cls)
    {
        public string Name { get; set; } = name;
        public ItemClasses Class { get; set; } = cls;

        public abstract void Use();
    }
}
