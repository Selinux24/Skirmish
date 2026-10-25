using Engine.Shaders.Properties;
using Vortice.Direct3D;
using System.IO;

namespace Engine.Common
{
    class ShaderIncludeManager : Include
    {
        public void Close(Stream stream)
        {
            stream.Dispose();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            byte[] o = (byte[])LibResources.ResourceManager.GetObject(Path.GetFileNameWithoutExtension(fileName));

            return new MemoryStream(o);
        }
    }
}
