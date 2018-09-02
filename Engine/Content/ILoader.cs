using System;

namespace Engine.Content
{
    public interface ILoader
    {
        ModelContent[] Load(string contentFolder, ModelContentDescription content);
    }
}
