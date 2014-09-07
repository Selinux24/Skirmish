using SharpDX;

namespace Common.Utils
{
    /// <summary>
    /// Intersecciones
    /// </summary>
    public static class Intersections
    {
        /// <summary>
        /// Obtiene si hay intersección entre el rayo y el plano
        /// </summary>
        /// <param name="ray">Rayo</param>
        /// <param name="plane">Plano</param>
        /// <param name="intersectionPoint">Punto de interseccion</param>
        /// <param name="distanceToPoint">Distancia al punto de interscción desde el origen del rayo</param>
        /// <param name="segmentMode">Indica si usar el rayo como un segmento en vez de como un rayo</param>
        /// <returns>Devuelve verdadero si hay intersección, y falso en el resto de los casos</returns>
        public static bool RayAndPlane(Ray ray, Plane plane, out Vector3? intersectionPoint, out float? distanceToPoint, bool segmentMode)
        {
            intersectionPoint = null;
            distanceToPoint = null;

            // Calcular la intersección del rayo con el plano
            float distance;
            if (ray.Intersects(ref plane, out distance))
            {
                if (segmentMode)
                {
                    if (distance > ray.Direction.Length())
                    {
                        return false;
                    }
                }

                // Obtener el punto de contacto en el plano y la distancia
                distanceToPoint = distance;
                intersectionPoint = ray.Position + (ray.Direction * distanceToPoint.Value);

                return true;
            }
            else
            {
                //No hay contacto
                return false;
            }
        }
        /// <summary>
        /// Obtiene si existe intersección entre el rayo y el triángulo
        /// </summary>
        /// <param name="ray">Rayo</param>
        /// <param name="tri">Triángulo</param>
        /// <param name="intersectionPoint">Punto de interseccion</param>
        /// <param name="distanceToPoint">Distancia al punto de interscción desde el origen del rayo</param>
        /// <param name="segmentMode">Indica si usar el rayo como un segmento en vez de como un rayo</param>
        /// <returns>Devuelve verdadero si hay intersección, y falso en el resto de los casos</returns>
        public static bool RayAndTriangle(Ray ray, Triangle tri, out Vector3? intersectionPoint, out float? distanceToPoint, bool segmentMode)
        {
            intersectionPoint = null;
            distanceToPoint = null;

            float denom = Vector3.Dot(tri.Plane.Normal, ray.Direction);
            if (denom == 0f)
            {
                return false;
            }

            float t = -(tri.Plane.D + Vector3.Dot(tri.Plane.Normal, ray.Position)) / denom;
            if (t <= 0.0f)
            {
                return false;
            }

            Vector3 intersection = ray.Position + (t * ray.Direction);
            if (Triangle.PointInTriangle(tri, intersection))
            {
                float distance = Vector3.Distance(ray.Position, intersection);

                if (segmentMode)
                {
                    if (distance <= ray.Direction.Length())
                    {
                        intersectionPoint = intersection;
                        distanceToPoint = distance;

                        return true;
                    }
                }
                else
                {
                    intersectionPoint = intersection;
                    distanceToPoint = distance;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Obtiene si hay intersección entre el rayo y la colección de triángulos
        /// </summary>
        /// <param name="ray">Rayo</param>
        /// <param name="triangleList">Lista de triángulos</param>
        /// <param name="intersectionPoint">Punto de interseccion</param>
        /// <param name="distanceToPoint">Distancia al punto de interscción desde el origen del rayo</param>
        /// <param name="segmentMode">Indica si usar el rayo como un segmento en vez de como un rayo</param>
        /// <returns>Devuelve verdadero si hay intersección, y falso en el resto de los casos</returns>
        public static bool RayAndTriangleSoup(Ray ray, Triangle[] triangleList, out Vector3? intersectionPoint, out float? distanceToPoint, bool segmentMode)
        {
            intersectionPoint = null;
            distanceToPoint = null;

            Vector3? closestPoint = null;
            float? closestDistance = null;

            for (int i = 0; i < triangleList.Length; i++)
            {
                Triangle t = triangleList[i];
                Vector3? point = null;
                float? distance = null;
                if (Intersections.RayAndTriangle(ray, t, out point, out distance, segmentMode))
                {
                    if (closestDistance.HasValue)
                    {
                        if (closestDistance > distance)
                        {
                            closestDistance = distance;
                            closestPoint = point;
                        }
                    }
                    else
                    {
                        closestDistance = distance;
                        closestPoint = point;
                    }
                }
            }

            if (closestPoint.HasValue && closestDistance.HasValue)
            {
                intersectionPoint = closestPoint;
                distanceToPoint = closestDistance;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
