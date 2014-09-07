using System.IO;
using SharpDX;
using SharpDX.D3DCompiler;

namespace Common.Utils
{
    public class ShaderIncludeManager : CallbackBase, Include
    {
        public void Close(Stream stream)
        {
            stream.Close();
            stream.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            object o = Properties.Resources.ResourceManager.GetObject(Path.GetFileNameWithoutExtension(fileName));

            return new MemoryStream((byte[])o);
        }
    }
}
