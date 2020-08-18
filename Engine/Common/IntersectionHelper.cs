
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
            var oneVolume = one.GetIntersectionVolume(detectionModeOne);
            var twoVolume = two.GetIntersectionVolume(detectionModeTwo);

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
            var oneVolume = one.GetIntersectionVolume(detectionModeOne);

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
            if (one is IntersectionVolumeSphere oneSph)
            {
                if (two is IntersectionVolumeSphere twoSph)
                {
                    return Intersection.SphereIntersectsSphere(oneSph, twoSph);
                }
                else if (two is IntersectionVolumeAxisAlignedBox twoBox)
                {
                    return Intersection.SphereIntersectsBox(oneSph, twoBox);
                }
                else if (two is IntersectionVolumeFrustum twoFrustum)
                {
                    return Intersection.SphereIntersectsFrustum(oneSph, twoFrustum);
                }
                else if (two is IntersectionVolumeMesh twoMesh)
                {
                    return Intersection.SphereIntersectsMesh(oneSph, (Triangle[])twoMesh, out Triangle _, out _, out _);
                }
            }
            else if (one is IntersectionVolumeAxisAlignedBox oneBox)
            {
                if (two is IntersectionVolumeSphere twoSph)
                {
                    return Intersection.SphereIntersectsBox(twoSph, oneBox);
                }
                else if (two is IntersectionVolumeAxisAlignedBox twoBox)
                {
                    return Intersection.BoxIntersectsBox(oneBox, twoBox);
                }
                else if (two is IntersectionVolumeFrustum twoFrustum)
                {
                    return Intersection.BoxIntersectsFrustum(oneBox, twoFrustum);
                }
                else if (two is IntersectionVolumeMesh twoMesh)
                {
                    return Intersection.BoxIntersectsMesh(oneBox, (Triangle[])twoMesh, out _);
                }
            }
            else if (one is IntersectionVolumeFrustum oneFrustum)
            {
                if (two is IntersectionVolumeSphere twoSph)
                {
                    return Intersection.SphereIntersectsFrustum(twoSph, oneFrustum);
                }
                else if (two is IntersectionVolumeAxisAlignedBox twoBox)
                {
                    return Intersection.BoxIntersectsFrustum(twoBox, oneFrustum);
                }
                else if (two is IntersectionVolumeFrustum twoFrustum)
                {
                    return Intersection.FrustumIntersectsFrustum(oneFrustum, twoFrustum);
                }
                else if (two is IntersectionVolumeMesh)
                {
                    return false;
                }
            }
            else if (one is IntersectionVolumeMesh oneMesh)
            {
                if (two is IntersectionVolumeSphere twoSph)
                {
                    return Intersection.SphereIntersectsMesh(twoSph, (Triangle[])oneMesh, out Triangle _, out _, out _);
                }
                else if (two is IntersectionVolumeAxisAlignedBox twoBox)
                {
                    return Intersection.BoxIntersectsMesh(twoBox, (Triangle[])oneMesh, out _);
                }
                else if (two is IntersectionVolumeFrustum)
                {
                    return false;
                }
                else if (two is IntersectionVolumeMesh twoMesh)
                {
                    return Intersection.MeshIntersectsMesh((Triangle[])oneMesh, (Triangle[])twoMesh, out _, out _);
                }
            }

            return false;
        }
    }
}
