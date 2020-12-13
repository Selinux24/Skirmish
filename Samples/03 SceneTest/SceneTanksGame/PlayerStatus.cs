using SharpDX;

namespace SceneTest.SceneTanksGame
{
    public class PlayerStatus
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public int MaxLife { get; set; }
        public int CurrentLife { get; set; }
        public int MaxMove { get; set; }
        public float CurrentMove { get; set; }
        public Color Color { get; set; }
        public float Health
        {
            get
            {
                return (float)CurrentLife / MaxLife;
            }
        }
        public uint TextureIndex
        {
            get
            {
                if (Health > 0.6666f)
                {
                    return 0;
                }
                else if (Health > 0)
                {
                    return 1;
                }

                return 2;
            }
        }

        public void NewTurn()
        {
            CurrentMove = MaxMove;
        }
    }
}
