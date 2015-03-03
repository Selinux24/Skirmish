
namespace GameLogic.Rules
{
    using GameLogic.Rules.Enum;

    public abstract class Item
    {
        public string Name { get; set; }
        public ItemClassEnum Class { get; set; }

        public Item(string name, ItemClassEnum cls)
        {
            this.Name = name;
            this.Class = cls;
        }

        public abstract void Use();
    }
}
