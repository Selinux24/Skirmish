
namespace GameLogic.Rules
{
    public abstract class Item
    {
        public string Name { get; set; }

        public abstract void Use();
    }
}
