
namespace TerrainSamples.SceneGrid.Rules
{
    using TerrainSamples.SceneGrid.Rules.Enum;

    public abstract class Item
    {
        public string Name { get; set; }
        public ItemClasses Class { get; set; }

        protected Item(string name, ItemClasses cls)
        {
            Name = name;
            Class = cls;
        }

        public abstract void Use();
    }
}
