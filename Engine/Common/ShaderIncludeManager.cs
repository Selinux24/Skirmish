using Engine.Shaders.Properties;
using Vortice.Direct3D;
using System.IO;
using SharpGen.Runtime;

namespace Engine.Common
{
    class ShaderIncludeManager : CallbackBase, Include
    {
        public void Close(Stream stream)
        {
            stream?.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            string resourceName = Path.GetFileNameWithoutExtension(fileName);

            byte[] bytes = (byte[])LibResources.ResourceManager.GetObject(resourceName);
            if (bytes == null)
            {
                throw new FileNotFoundException($"No se encontró el shader include: {fileName}");
            }

            return new MemoryStream(bytes);
        }
    }
}
