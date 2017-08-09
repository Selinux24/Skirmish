namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
	/// Settings for a particular crowd agent
	/// </summary>
	public struct AgentParams
    {
        public float Radius { get; set; }
        public float Height { get; set; }
        public float MaxAcceleration { get; set; }
        public float MaxSpeed { get; set; }
        public float CollisionQueryRange { get; set; }
        public float PathOptimizationRange { get; set; }
        public float SeparationWeight { get; set; }
        public UpdateFlags UpdateFlags { get; set; }
        public byte ObstacleAvoidanceType { get; set; }
        public byte QueryFilterType { get; set; }
        public float TriggerRadius
        {
            get
            {
                return this.Radius * 2.25f;
            }
        }
        public float UpdateThreshold
        {
            get
            {
                var uth = this.CollisionQueryRange * 0.25f;
                return uth * uth;
            }
        }
    }
}
