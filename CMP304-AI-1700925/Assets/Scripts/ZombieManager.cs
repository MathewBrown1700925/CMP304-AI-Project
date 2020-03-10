using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//manager class to handle the Genetic Algorithm operations
public class ZombieManager : MonoBehaviour
{
    //Public Members
    //
    public bool training = false;
    //Population Size must be even otherwise is set to 20 further in code
    public int popSize = 50;
    //Editing of time scale can cause performance issues so value capped at 20
    [Range(1,20)]
    public int timeScale = 1;
    //Mutation Chance ranging from 0%(no mutation) to 100%(entirely random results)
    [Range(0.0f, 100.0f)]
    public float mutationChance = 1.0f;
    //Training Time in Seconds
    public float trainingTime = 15.0f;
    //Spawn Point for zombie agents to be instantiated around
    public Vector3 spawnPoint;
    //Goal object to pathfind to
    public GameObject goal;
    //Average Fitness Variable
    public float averageFitness;
    //Generation Count
    public int genNumber = 0;
    //Neuron Configiration for Layers
    private int[] layers = new int[] { 5, 4, 4, 4 };
    //list of Neural Networks currently being processed
    private List<NeuralNetwork> networks;
    //Agent Prefab
    public GameObject zombie;
    //Grid Reference for A* Pathfinding Algorithm
    public Grid astarGrid;
    //Switch to handle if reward is altered by the presence of collisions
    public bool collisionReward = true;
    //UI Elements
    public UnityEngine.UI.Text generationText;
    public UnityEngine.UI.Text averageFitnessText;
    public UnityEngine.UI.Text timeScaleText;
    public UnityEngine.UI.Text mutationRateText;
    public UnityEngine.UI.Text timeText;
    //CSV Filename
    public string filename;
    //Private Members
    //A* pathfinding Object
    private Pathfinding pathfinding;
    //Agent list
    private List<ZombieANNScript> zombieList = null;
    //Timing Variables
    private float currentTime = 0.0f;
    private float timeAtStart = 0.0f;
    //Output Data Lists
    private List<float> genList;
    private List<float> timeList;
    private List<float> fitnessList;
    //Highest Fitness Variable
    private float highestFitness;

    //Function to stop training of a current generation allowing a new generation to begin
    //Called via a timer
    void StopTraining()
    {
        training = false;
    }
    //Initialisation of variables and objects
    private void Start()
    {
        UnityEngine.Time.timeScale = timeScale;
        astarGrid = GetComponent<Grid>();
        pathfinding = GetComponent<Pathfinding>();
        highestFitness = 0;
        averageFitness = 0;
        timeAtStart = Time.time;
        currentTime = Time.time - timeAtStart;
        //
        genList = new List<float>();
        timeList = new List<float>();
        fitnessList = new List<float>();
    }


    void Update()
    {
        //Dynamic changing of time scale for purposes of debugging
        UnityEngine.Time.timeScale = timeScale;
        //Update time variable
        currentTime = Time.time - timeAtStart;
        //Genetic Algorithm
        if (training == false)
        {
            //Upon initial run InitZombies is run to initialise neural networks for the zombie agents
            if (genNumber == 0)
            {
                InitZombies();
            } else
            {
                //Reset average fitness after every generation
                averageFitness = 0;
               //
                for (int l = 0; l < popSize; l++)
                {
                    //Pass data required for A* pathfinding 
                    pathfinding.StartPosition = zombieList[l].transform;
                    astarGrid.StartPosition = zombieList[l].transform;
                    //Find optimal path from current agent position
                    pathfinding.FindPath(zombieList[l].transform.position, goal.transform.position);
                    //Get path distance from agent to goal
                    zombieList[l].distanceFromGoal = pathfinding.FinalPath.Count;
                    //Initialise Fitness Variables
                    float fitnessQuantity = 0f;
                    float collisionNum = zombieList[l].noOfCollisions * 2;
                    //
                    if (collisionReward == false)
                    {
                        collisionNum = 1;
                    }
                    if (collisionNum < 1)
                    {
                        collisionNum = 1;
                    }
                    //Reward seperately to avoid potential division by < 0
                    if (zombieList[l].distanceFromGoal > 2)
                    {
                        fitnessQuantity = 100000 / zombieList[l].distanceFromGoal;
                    }else
                    {
                        fitnessQuantity = 100000;
                    }
                    //Set fitness of the network and add the quantity to the average
                    networks[l].SetFitness(fitnessQuantity);
                    averageFitness += fitnessQuantity;
                }
                //Divide average by number of agents/networks
                averageFitness /= popSize;
                //List for the next generation of agents
                List<NeuralNetwork> nextGen = new List<NeuralNetwork>();
                //Create children for the next generation
                //Creates 10 less than current generation to allow best solutions to survive
                for (int i = 0; i < popSize-10; i++)
                {
                    //Roulette Wheel reproduction to give better of chance of reproduction between higher fitness networks
                    NeuralNetwork child = RouletteWheelSelection().Reproduction(RouletteWheelSelection());
                    //Apply simple mutation to avoid trapping in similar patterns
                        child.MutateAlternate(mutationChance);
                    //Clear fitness value and add to next generation
                    child.SetFitness(0.0f);
                    nextGen.Add(child);

                }
                //Sort network list by ascending order of fitness
                networks.Sort();
                //Store the highest fitness value of the generation
                highestFitness = networks[popSize - 1].GetFitness();
                //Replace lower fitness solutions with next generation of networks
                for (int i = 0; i < popSize-10; i++)
                {
                    networks[i] = nextGen[i];
                }
            }
            //Incriment Generation Counter
            genNumber++;
            //Update UI
            generationText.text = "Generation Number:" + genNumber.ToString();
            averageFitnessText.text = "Average Fitness:" + averageFitness.ToString();
            timeScaleText.text = "World Time Scale:" + UnityEngine.Time.timeScale.ToString();
            mutationRateText.text = "Mutation Rate:" + mutationChance.ToString() + "%";
            timeText.text = "Time:" + currentTime.ToString();
            //Begin training generation for trainingTime quantity of seconds
            training = true;
            Invoke("StopTraining", trainingTime);
            //Spawn agents with newly created networks
            SpawnZombies();
            //Add data to output lists and append to a CSV File
            genList.Add(genNumber);
            fitnessList.Add(averageFitness);
            timeList.Add(currentTime);
            PrintCSV(genList.ToArray(),timeList.ToArray(),fitnessList.ToArray(),genList.Count);
        }
    }
    //Spawn agents into scene
    private void SpawnZombies()
    {
        //Clear list if agents already exist
        if(zombieList != null)
        {
            for (int i = 0; i < zombieList.Count; i++)
            {
                GameObject.Destroy(zombieList[i].gameObject);
            }
        }
        //Create new list populating with agents being spawned at random points around a chosen world point
        zombieList = new List<ZombieANNScript>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 spawnPos = new Vector3(spawnPoint.x +UnityEngine.Random.Range(-10.0f, 10.0f), spawnPoint.y, spawnPoint.z+UnityEngine.Random.Range(-10.0f, 10.0f));
            Transform spawnTransform = zombie.transform;
            spawnTransform.position = spawnPos;
            ZombieANNScript zombieObj = (Instantiate(zombie, new Vector3(spawnPoint.x + UnityEngine.Random.Range(-10.0f, 10.0f), spawnPoint.y, spawnPoint.z + UnityEngine.Random.Range(-10.0f, 10.0f)),zombie.transform.rotation) as GameObject).GetComponent<ZombieANNScript>();
            //Assign neural network and goal object
            zombieObj.Init(networks[i], goal);
            zombieList.Add(zombieObj);
        }
    }

    //Initialise Neural Networks
    private void InitZombies()
    {
        //Ensure networks are even
        if (popSize % 2 != 0)
        {
            popSize = 20;
        }
        networks = new List<NeuralNetwork>();

        for (int i = 0; i < popSize; i++)
        {
            NeuralNetwork tempNetwork = new NeuralNetwork(layers);
            tempNetwork.Mutate();
            networks.Add(tempNetwork);
        }
    }

    //Roulette Wheel Selection of parents
    private NeuralNetwork RouletteWheelSelection()
    {//Get total sum of fitness of all agents
        float fitnessTotalSum = 0.0f;
        for (int i = 0; i < popSize; i++)
        {
            fitnessTotalSum += networks[i].GetFitness();
        }
        //Find a random sum of fitness and loop through the network list taking a sum until the random sum is reached/surpassed
        //The current agent when that occurs is a parent
        float randomSum = UnityEngine.Random.Range(0.0f, fitnessTotalSum);
        float partialFitnessSum = 0.0f;
        for (int i = 0; i < popSize; i++)
        {
            partialFitnessSum += networks[i].GetFitness();
            if (partialFitnessSum >= randomSum)
            {
                return networks[i];
            }
        }
        //Return null if no parent was found
        return null;
    }

    //Print data to CSV File
    private void PrintCSV(float[] firstColumn,float[] secondColumn,float[] thirdColumn,int noOfRows)
    {
        //Stringbuilder object to format data for output
        System.Text.StringBuilder csvFile = new System.Text.StringBuilder();
        //Column Headers
        csvFile.AppendLine("Generations,Time,AvgFitness");
        //Put data into comma seperated lines and append
        for (int i = 0; i <noOfRows;i++)
        {
            string col1 = firstColumn[i].ToString();
            string col2 = secondColumn[i].ToString();
            string col3 = thirdColumn[i].ToString();
            string line = col1 + "," + col2 + "," + col3;
            csvFile.AppendLine(line);
        }
        //Set filepath and write file
        string filepath = Application.dataPath + "/" + filename;
        System.IO.File.WriteAllText(filepath,csvFile.ToString());
    }
}
