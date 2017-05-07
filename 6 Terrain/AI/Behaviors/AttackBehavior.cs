using Engine;

namespace TerrainTest.AI.Behaviors
{
    class AttackBehavior : Behavior
    {
        public Agent Target { get; private set; }

        public AttackBehavior(Agent agent) : base(agent)
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (this.Target != null)
            {
                if (this.Agent.CanAttack)
                {
                    this.Agent.Attack(this.Target);
                }

                if (this.Target.Life <= 0)
                {
                    this.Active = false;
                }
            }
        }

        public void SetTarget(Agent target)
        {
            this.Target = target;
            this.Active = true;
        }

        public override string ToString()
        {
            return "Attack";
        }
    }
}
