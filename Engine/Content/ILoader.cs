using System;

namespace Engine.Content
{
    public interface ILoader: IDisposable
    {
        ModelContent[] Load(string contentFolder, ModelContentDescription content);
    }
}
