﻿using Engine;
using SharpDX;

namespace AISamples.SceneCodingWithRadu
{
    class Level
    {
        private readonly float[] inputs;
        private readonly float[] outputs;
        private readonly float[] biases;
        private readonly float[][] weights;

        public Level(int inputCount, int outputCount)
        {
            inputs = new float[inputCount];
            outputs = new float[outputCount];
            biases = new float[outputCount];

            weights = new float[inputCount][];
            for (int i = 0; i < inputCount; i++)
            {
                weights[i] = new float[outputCount];
            }

            Randomize(this);
        }

        static void Randomize(Level level)
        {
            for (int i = 0; i < level.inputs.Length; i++)
            {
                for (int o = 0; o < level.outputs.Length; o++)
                {
                    level.weights[i][o] = Helper.RandomGenerator.NextFloat(-1, 1);
                }
            }

            for (int b = 0; b < level.biases.Length; b++)
            {
                level.biases[b] = Helper.RandomGenerator.NextFloat(-1, 1);
            }
        }

        public static float[] FeedForward(float[] givenInputs, Level level)
        {
            for (int i = 0; i < level.inputs.Length; i++)
            {
                level.inputs[i] = givenInputs[i];
            }

            for (int o = 0; o < level.outputs.Length; o++)
            {
                float sum = 0;
                for (int i = 0; i < level.inputs.Length; i++)
                {
                    sum += level.inputs[i] * level.weights[i][o];
                }

                if (sum > level.biases[o])
                {
                    level.outputs[o] = 1;
                }
                else
                {
                    level.outputs[o] = 0;
                }
            }

            return level.outputs;
        }

        public void Reset()
        {
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i] = 0f;
            }
        }

        public int GetInputCount()
        {
            return inputs.Length;
        }
        public float[] GetInputs()
        {
            return [.. inputs];
        }
        public float GetInput(int index)
        {
            return inputs[index];
        }
        public int GetOutputCount()
        {
            return outputs.Length;
        }
        public float[] GetOutputs()
        {
            return [.. outputs];
        }
        public float GetOutput(int index)
        {
            return outputs[index];
        }
        public float[] GetBiases()
        {
            return [.. biases];
        }
        public float GetBias(int index)
        {
            return biases[index];
        }
        public float GetWeight(int i, int o)
        {
            return weights[i][o];
        }
    }
}
