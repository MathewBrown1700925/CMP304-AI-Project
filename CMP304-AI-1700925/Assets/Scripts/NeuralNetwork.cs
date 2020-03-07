using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    //Public Parameters
    public float constantBias = 0.25f;
    //Layer index 1 = hidden layer
    private int[] layers;
    private float[][] neurons;
    private float[][][] weightings;
    public NeuralNetwork(int[] pLayers)
    {
        //Initialise layers
        layers = new int[pLayers.Length];
        for (int i = 0; i < pLayers.Length; i++)
        {
            layers[i] = pLayers[i];
        }
        //Initialise Layers & Weightings
        InitialiseNeurons();
        InitialiseWeights();
    }

    //Function to initialise neurons
    private void InitialiseNeurons()
    {
        //Create list of floats which is then used to create neurons for each layer
        List<float[]> neuronList = new List<float[]>();
        for(int i = 0; i < layers.Length; i++)
        {
            neuronList.Add(new float[layers[i]]);
        }
        //Convert list into an array to copy data to neuron member array
        neurons = neuronList.ToArray();
    }

    private void InitialiseWeights()
    {
        //Create list of weightings which is then used to create weightings for each weighted node
        List<float[][]> weightingList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWieghtingsList = new List<float[]>();
            int previousLayerNeuronsCount = layers[i - 1];
            for (int p = 0; p < neurons.Length; p++)
            {
                float[] neuronWeightings = new float[previousLayerNeuronsCount];
                //
                for (int l = 0; l < previousLayerNeuronsCount; l++)
                {
                    neuronWeightings[l] = Random.Range(0.0f, 1.0f) - 0.5f;
                }
                layerWieghtingsList.Add(neuronWeightings);
            }
            weightingList.Add(layerWieghtingsList.ToArray());
        }

        weightings = weightingList.ToArray();

    }

    public float[] ForwardProp(float[] inputs)
    {
        //Loop through inputs placing each value within a neuron in the first layer(the input layer) of the ANN
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        //Iterate through every layer apart from the input layer
        for (int i = 1; i < layers.Length; i++)
        {
            //Iterate through every neuron in the layer
            for (int l = 0; l < neurons[i].Length; l++)
            {
                float neuronVal = constantBias;
                //Iterate through every neuron in the PREVIOUS layer
                for (int p = 0; p < neurons[i-1].Length; p++)
                {
                    neuronVal += weightings[i - 1][l][p] * neurons[i - 1][p];
                }
                //Activation
                neurons[i][l] = (float)System.Math.Tanh(neuronVal);
            }
        }

        //Return output layer
        return neurons[neurons.Length-1];
    }

    public void Mutate()
    {
        for (int i = 0; i < weightings.Length; i++)
        {
            for ( int l = 0; l < weightings[i].Length; l++)
            {
                for (int p = 0; p < weightings[i][l].Length; p++)
                {
                    float currentWeight = weightings[i][l][p];
                    //
                    float randNum = Random.Range(0.0f, 1000.0f);
                    if (randNum <=2f)
                    {
                        currentWeight *= -1f;
                    } else if (randNum <= 4f)
                    {
                        currentWeight = Random.Range(-0.5f, 0.5f);
                    } else if (randNum <= 6f)
                    {
                        currentWeight *= Random.Range(0.0f, 1.0f) + 1f;
                    } else if (randNum <= 8f)
                    {
                        currentWeight *= Random.Range(0.0f, 1.0f);
                    }
                    //
                    weightings[i][l][p] = currentWeight;
                }
            }
        }
    }


    //Deep Copy
    public NeuralNetwork(NeuralNetwork copyNet)
    {
        layers = new int[copyNet.layers.Length];
        for (int i = 0; i < copyNet.layers.Length; i++)
        {
            layers[i] = copyNet.layers[i];
        }
        InitialiseNeurons();
        InitialiseWeights();
        CopyWeights(copyNet.weightings);
    }

    private void CopyWeights(float[][][] copyWeightings)
    {
        for (int i = 0; i < weightings.Length; i++)
        {
            for (int l = 0; l < weightings[i].Length; l++)
            {
                for (int p = 0; p < weightings[i][l].Length; p++)
                {
                    weightings[i][l][p] = copyWeightings[i][l][p];
                }
            }
        }
    }
}

