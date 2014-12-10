using System.IO;

namespace Engine.Content
{
    public static class ContentManager
    {
        public static string FindContent(string contentFolder, string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }
            else if (File.Exists(resourcePath))
            {
                return resourcePath;
            }
            else
            {
                resourcePath = Path.Combine(contentFolder, resourcePath);
                if (File.Exists(resourcePath))
                {
                    return resourcePath;
                }
                else
                {
                    throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePath);
                }
            }
        }

        public static string[] FindContent(string contentFolder, string[] resourcePaths)
        {
            if (resourcePaths != null && resourcePaths.Length > 0)
            {
                for (int i = 0; i < resourcePaths.Length; i++)
                {
                    if (!File.Exists(resourcePaths[i]))
                    {
                        string resourcePath = Path.Combine(contentFolder, resourcePaths[i]);
                        if (File.Exists(resourcePath))
                        {
                            resourcePaths[i] = resourcePath;
                        }
                        else
                        {
                            throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePath);
                        }
                    }
                }
            }

            return resourcePaths;
        }
    }
}
