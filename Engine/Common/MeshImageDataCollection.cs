using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Mesh image data collection
    /// </summary>
    public class MeshImageDataCollection : IEnumerable<(string Name, MeshImageData Data)>
    {
        /// <summary>
        /// Internal list
        /// </summary>
        private readonly List<(string Name, MeshImageData Data)> imageList = new();

        /// <summary>
        /// Gets the internal image name list
        /// </summary>
        public string[] ImageNames
        {
            get
            {
                return imageList.Select(m => m.Name).ToArray();
            }
        }

        /// <inheritdoc/>
        public IEnumerator<(string Name, MeshImageData Data)> GetEnumerator()
        {
            return imageList.GetEnumerator();
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return imageList.GetEnumerator();
        }

        /// <summary>
        /// Adds a new image data to the collection
        /// </summary>
        /// <param name="name">Image name</param>
        /// <param name="imageData">Image data</param>
        public void Add(string name, MeshImageData imageData)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var current = imageList.FindIndex(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            if (current < 0)
            {
                imageList.Add((name, imageData));

                return;
            }

            imageList[current] = (name, imageData);
        }
        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            imageList.Clear();
        }
        /// <summary>
        /// Gets the image with the specified name
        /// </summary>
        /// <param name="name">Image name</param>
        public MeshImageData GetImageData(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            int index = imageList.FindIndex(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            return imageList[index].Data;
        }
        /// <summary>
        /// Gets the resource of the specified image
        /// </summary>
        /// <param name="name">Image name</param>
        public EngineShaderResourceView GetImage(string name)
        {
            return GetImageData(name)?.Texture?.Resource;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Images: {ImageNames?.Join("|")}";
        }
    }
}
