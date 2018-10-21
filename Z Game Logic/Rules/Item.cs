
namespace GameLogic.Rules
{
    using GameLogic.Rules.Enum;

    public abstract class Item
    {
        public string Name { get; set; }
        public ItemClasses Class { get; set; }

        protected Item(string name, ItemClasses cls)
        {
            this.Name = name;
            this.Class = cls;
        }

        public abstract void Use();
    }
}
