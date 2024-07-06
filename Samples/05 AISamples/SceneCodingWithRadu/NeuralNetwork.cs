using System;

namespace AISamples.SceneCodingWithRadu
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

        public float[] GetOutputs()
        {
            return [.. outputs];
        }
    }
}
