namespace Engine.PathFinding.NavMesh.Crowds
{
    public struct ObstacleAvoidanceParams
    {
        public float VelBias;
        public float WeightDesVel;
        public float WeightCurVel;
        public float WeightSide;
        public float WeightToi;
        public float HorizTime;
        public int GridSize;
        public int AdaptiveDivs;
        public int AdaptiveRings;
        public int AdaptiveDepth;
    }
}
