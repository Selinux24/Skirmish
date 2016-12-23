using System;
using System.Collections.Generic;

namespace Engine.Common
{
    public class VertexData2
    {
        private List<VertexDataChannel2> channelList = new List<VertexDataChannel2>();

        public void AddChannel<T>(string name, T value) where T : struct
        {
            this.channelList.Add(new VertexDataChannel2<T>() { Name = name, Value = value });
        }
        public void RemoveChannel(string name)
        {
            int index = this.channelList.FindIndex(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                this.channelList.RemoveAt(index);
            }
        }
        public T SetValue<T>(string name) where T : struct
        {
            var channel = this.channelList.Find(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (channel != null)
            {
                return ((VertexDataChannel2<T>)channel).Value;
            }
            else
            {
                return default(T);
            }
        }
        public bool HasChannel(string name)
        {
            int index = this.channelList.FindIndex(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

            return (index >= 0);
        }
    }

    public abstract class VertexDataChannel2
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class VertexDataChannel2<T> : VertexDataChannel2 where T : struct
    {
        public new T Value
        {
            get
            {
                return (T)base.Value;
            }
            set
            {
                base.Value = value;
            }
        }
    }
}
