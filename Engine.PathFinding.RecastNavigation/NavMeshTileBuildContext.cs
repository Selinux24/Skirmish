
namespace Engine.PathFinding.RecastNavigation
{
    public class NavMeshTileBuildContext
    {
        public TileCacheLayer Layer { get; set; }
        public TileCacheContourSet LCSet { get; set; }
        public TileCachePolyMesh LMesh { get; set; }

        public void SetLayerRegs(int[] layerRegs, int regId)
        {
            var layer = Layer;
            layer.RegCount = regId;
            layer.Regs = layerRegs;
            Layer = layer;
        }
    }
}
