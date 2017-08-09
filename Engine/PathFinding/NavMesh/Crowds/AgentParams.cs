namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
	/// Settings for a particular crowd agent
	/// </summary>
	public struct AgentParams
    {
        public float Radius;
        public float Height;
        public float MaxAcceleration;
        public float MaxSpeed;
        public float CollisionQueryRange;
        public float PathOptimizationRange;
        public float SeparationWeight;
        public UpdateFlags UpdateFlags;
        public byte ObstacleAvoidanceType;
        public byte QueryFilterType;
        public float TriggerRadius
        {
            get
            {
                return this.Radius * 2.25f;
            }
        }
    }
}
