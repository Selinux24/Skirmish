using Engine;
using System;
using System.IO;
using System.Linq;

namespace AISamples.SceneCWRSelfDrivingCar
{
    class NeuralNetwork
    {
        private readonly Level[] levels;
        private readonly float[] outputs;

        public NeuralNetwork(int[] neuronCounts)
        {
            levels = new Level[neuronCounts.Length - 1];
            for (int i = 0; i < neuronCounts.Length - 1; i++)
            {
                levels[i] = new Level(neuronCounts[i], neuronCounts[i + 1]);
            }
            outputs = new float[neuronCounts[^1]];
        }

        public void FeedForward(float[] givenInputs)
        {
            var levelOutputs = Level.FeedForward(givenInputs, levels[0]);

            for (int i = 1; i < levels.Length; i++)
            {
                levelOutputs = Level.FeedForward(levelOutputs, levels[i]);
            }

            Array.Copy(levelOutputs, outputs, levelOutputs.Length);
        }

        public void Reset()
        {
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i].Reset();
            }

            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i] = 0f;
            }
        }

        public int GetLevelCount()
        {
            return levels.Length;
        }
        public Level[] GetLevels()
        {
            return [.. levels];
        }
        public Level GetLevel(int index)
        {
            return levels[index];
        }
        public float[] GetOutputs()
        {
            return [.. outputs];
        }

        public void Save(string fileName)
        {
            var file = new NeuralNetworkFile()
            {
                Levels = levels.Select(l => l.ToFile()).ToArray(),
                Outputs = [.. outputs]
            };

            SerializationHelper.SerializeJsonToFile(file, fileName);
        }
        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            var file = SerializationHelper.DeserializeJsonFromFile<NeuralNetworkFile>(fileName);

            Level[] levelArray = file.Levels.Select(Level.FromFile).ToArray();
            float[] outputArray = [.. file.Outputs];

            Array.Copy(levelArray, levels, levelArray.Length);
            Array.Copy(outputArray, outputs, outputArray.Length);
        }

        public void Mutate(float amount)
        {
            foreach (var level in levels)
            {
                level.Mutate(amount);
            }
        }
    }

    class NeuralNetworkFile
    {
        public LevelFile[] Levels { get; set; }
        public float[] Outputs { get; set; }
    }
}
