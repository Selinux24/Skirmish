using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Helper class for intersections detector
    /// </summary>
    public static class IntersectionHelper
    {
        /// <summary>
        /// Gets whether two intersectables have intersection or not
        /// </summary>
        /// <param name="one">Object one</param>
        /// <param name="detectionModeOne">Detection mode for one</param>
        /// <param name="two">Object two</param>
        /// <param name="detectionModeTwo">Detection mode for two</param>
        /// <returns>Returns true if have intersection</returns>
        public static bool Intersects(IIntersectable one, IntersectDetectionMode detectionModeOne, IIntersectable two, IntersectDetectionMode detectionModeTwo)
        {
            var oneVolume = one?.GetIntersectionVolume(detectionModeOne);
            var twoVolume = two?.GetIntersectionVolume(detectionModeTwo);

            return Intersects(oneVolume, twoVolume);
        }
        /// <summary>
        /// Gets whether the intersectable and the volumen have intersection or not
        /// </summary>
        /// <param name="one">Object one</param>
        /// <param name="detectionModeOne">Detection mode for one</param>
        /// <param name="twoVolume">Volume</param>
        /// <returns>Returns true if have intersection</returns>
        public static bool Intersects(IIntersectable one, IntersectDetectionMode detectionModeOne, IIntersectionVolume twoVolume)
        {
            var oneVolume = one?.GetIntersectionVolume(detectionModeOne);

            return Intersects(oneVolume, twoVolume);
        }
        /// <summary>
        /// Gets whether two volumes have intersection or not
        /// </summary>
        /// <param name="one">Volume one</param>
        /// <param name="two">Volume two</param>
        /// <returns>Returns true if have intersection</returns>
        public static bool Intersects(IIntersectionVolume one, IIntersectionVolume two)
        {
            if (one == null || two == null)
            {
                return false;
            }

            if (two is IntersectionVolumeSphere twoSph)
            {
                return one.Contains(twoSph) != ContainmentType.Disjoint;
            }
            
            if (two is IntersectionVolumeAxisAlignedBox twoBox)
            {
                return one.Contains(twoBox) != ContainmentType.Disjoint;
            }
            
            if (two is IntersectionVolumeFrustum twoFrustum)
            {
                return one.Contains(twoFrustum) != ContainmentType.Disjoint;
            }
            
            if (two is IntersectionVolumeMesh twoMesh)
            {
                return one.Contains((Triangle[])twoMesh) != ContainmentType.Disjoint;
            }

            return false;
        }
    }
}
