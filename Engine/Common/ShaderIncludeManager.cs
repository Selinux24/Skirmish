using SharpDX;
using SharpDX.D3DCompiler;
using System.IO;

namespace Engine.Common
{
    using Engine.Properties;

    class ShaderIncludeManager : CallbackBase, Include
    {
        public void Close(Stream stream)
        {
            stream.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            byte[] o = (byte[])Resources.ResourceManager.GetObject(Path.GetFileNameWithoutExtension(fileName));

            return new MemoryStream(o);
        }
    }
}
