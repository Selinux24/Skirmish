using Engine.Shaders.Properties;
using SharpGen.Runtime;
using System.IO;

namespace Engine.Common
{
    class ShaderIncludeManager : CallbackBase, Vortice.Direct3D.Include
    {
        public Stream Open(Vortice.Direct3D.IncludeType type, string fileName, Stream parentStream)
        {
            string resourceName = Path.GetFileNameWithoutExtension(fileName);

            byte[] bytes = 
                (byte[])LibResources.ResourceManager.GetObject(resourceName) ?? 
                throw new FileNotFoundException($"No se encontró el shader include: {fileName}");

            return new MemoryStream(bytes);
        }

        public void Close(Stream stream)
        {
            stream?.Dispose();
        }
    }
}
