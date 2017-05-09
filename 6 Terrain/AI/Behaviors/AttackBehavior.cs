using Engine;
using SharpDX;

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
                if (this.Target.Life <= 0)
                {
                    this.Active = false;
                }
                else if (Vector3.Distance(this.Agent.Model.Manipulator.Position, this.Target.Model.Manipulator.Position) >= this.Agent.CurrentWeapon.Range)
                {
                    this.Active = false;
                }
                else if (this.Agent.CanAttack)
                {
                    this.Agent.Attack(this.Target);
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
