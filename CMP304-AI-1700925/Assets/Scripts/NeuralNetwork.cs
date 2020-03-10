using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//Neural Network Class
public class NeuralNetwork : IComparable<NeuralNetwork>
{
    //Public Parameters
    public float constantBias = 0.25f;
    //Layer Index
    //0 = Input
    //1-2 Hidden
    //3 Output
    private int[] layers;
    private float[][] neurons;
    private float[][][] weightings;
    private float fitness;
    //Constructor taking the quantity of neurons per layer as a parameter
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
                //As weightings will be found by the Genetic Algorithm, random weightings can be used for the purpose of initialisation
                for (int l = 0; l < previousLayerNeuronsCount; l++)
                {
                    neuronWeightings[l] = UnityEngine.Random.Range(0.0f, 1.0f) - 0.5f;
                }
                layerWieghtingsList.Add(neuronWeightings);
            }
            weightingList.Add(layerWieghtingsList.ToArray());
        }

        weightings = weightingList.ToArray();

    }
    //Forward Propogation function to pass data through layers while applying weightings
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
                //Activation Function
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
            for (int j = 0; j < weightings[i].Length; j++)
            {
                for (int k = 0; k < weightings[i][j].Length; k++)
                {
                    float weight = weightings[i][j][k];

                    //mutate weight value 
                    float randomNumber = UnityEngine.Random.Range(0f, 100f);

                    if (randomNumber <= 2f)
                    { //if 1
                      //flip sign of weight
                        weight *= -1f;
                    }
                    else if (randomNumber <= 4f)
                    { //if 2
                      //pick random weight between -1 and 1
                        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                    else if (randomNumber <= 6f)
                    { //if 3
                      //randomly increase by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    }
                    else if (randomNumber <= 8f)
                    { //if 4
                      //randomly decrease by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                    }

                    weightings[i][j][k] = weight;
                }
            }
        }
    }

    //Alternate mutation function that has a chance to randomise a single weight in a layer
    public void MutateAlternate(float mutationChance)
    {
        for (int i = 0; i < weightings.Length; i++)
        {
            for (int l = 0; l < weightings[i].Length; l++)
            {
                for (int p = 0; p < weightings[i][l].Length; p++)
                {
                    float currentWeight = weightings[i][l][p];
                    //
                    float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
                    if (randNum <= mutationChance)
                    {
                        currentWeight = UnityEngine.Random.Range(-0.5f,0.5f);
                        weightings[i][l][p] = currentWeight;
                        goto END;
                    }
                }
            }
        }
    END:;
    }

    //Reproduction Function that defines a random point in the network taking some weightings from one parent and some from another
    public NeuralNetwork Reproduction(NeuralNetwork otherParent)
    {
        NeuralNetwork child = new NeuralNetwork(layers);
        bool crossoverPointReached = false;
        //Calculate random point
        int[] randomPoint = new int[3];
        randomPoint[0] = UnityEngine.Random.Range(0, weightings.Length);
        randomPoint[1] = UnityEngine.Random.Range(0, weightings[0].Length);
        randomPoint[2] = UnityEngine.Random.Range(0, weightings[0][0].Length);
        for (int i = 0; i < weightings.Length; i++)
        {
            for (int l = 0; l < weightings[i].Length; l++)
            {
                for (int p = 0; p < weightings[i][l].Length; p++)
                {
                    if ((i == randomPoint[0]) && (l == randomPoint[1]) && (p == randomPoint[2]))
                    {
                        crossoverPointReached = true;
                    }
                    if (crossoverPointReached == true)
                    {
                        child.weightings[i][l][p] = otherParent.weightings[i][l][p];
                    } else
                    {
                        child.weightings[i][l][p] = weightings[i][l][p];
                    }
                }
            }
        }
        return child;
    }


    //Deep Copy Network
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
    //Deep Copy Weights
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
   //Incriment function for altering fitness
    public void IncrimentFitness(float fitnessAmount)
    {
        fitness += fitnessAmount;
    }
    //Setter to set fitness to an exact value
    public void SetFitness(float fitnessAmount)
    {
        fitness = fitnessAmount;
    }
    //Getter for fitness
    public float GetFitness()
    {
        return fitness;
    }
    //CompareTo function for the purposes of IComparable operations
    public int CompareTo(NeuralNetwork otherNetwork)
    {
        if (otherNetwork == null)
        {
            return 1;
        }
        if (fitness > otherNetwork.fitness)
        {
            return 1;
        } else if (fitness < otherNetwork.fitness)
        {
            return -1;
        } else
        {
            return 0;
        }
    }
 

}

