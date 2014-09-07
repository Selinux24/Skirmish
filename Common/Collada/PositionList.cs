using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SharpDX;

namespace Common.Collada
{
    [Serializable]
    public class PositionList
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("count")]
        public int Count { get; set; }
        [XmlText]
        public string PositionsText
        {
            get { return Dae.Convert(this.Positions); }
            set { this.Positions = Dae.ConvertFloatArrayList(value); }
        }
        [XmlIgnore]
        public List<List<float>> Positions { get; set; }

        public Vector3[] GetData(int offset, int stride)
        {
            List<Vector3> result = new List<Vector3>();

            for (int i = 0; i < this.Positions.Count; i++)
            {
                Vector3 v = new Vector3(
                    this.Positions[i][offset + 0],
                    this.Positions[i][offset + 1],
                    this.Positions[i][offset + 2]);

                result.Add(v);
            }

            return result.ToArray();
        }
    }
}
