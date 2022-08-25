using Shaders.Properties;
using SharpDX;
using SharpDX.D3DCompiler;
using System.IO;

namespace Engine.Common
{
    class ShaderIncludeManager : CallbackBase, Include
    {
        public void Close(Stream stream)
        {
            stream.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            byte[] o = (byte[])LibResources.ResourceManager.GetObject(Path.GetFileNameWithoutExtension(fileName));

            return new MemoryStream(o);
        }
    }
}
