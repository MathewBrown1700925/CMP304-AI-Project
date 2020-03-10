using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public bool training = false;
    public int popSize = 50;
    [Range(1,100)]
    public int timeScale = 1;
    [Range(0.0f, 100.0f)]
    public float mutationChance = 1.0f;
    public float trainingTime = 15.0f;
    public Vector3 spawnPoint;
    public GameObject goal;
    public float highestFitness;
    public float averageFitness;
    public int genNumber = 0;
    private int[] layers = new int[] { 5, 4, 4, 4 };
    private List<NeuralNetwork> networks;
    public GameObject zombie;
    public Grid astarGrid;
    public bool collisionReward = true;
    Pathfinding pathfinding;
    private List<ZombieANNScript> zombieList = null;
    public UnityEngine.UI.Text generationText;
    public UnityEngine.UI.Text averageFitnessText;
    public UnityEngine.UI.Text timeScaleText;
    public UnityEngine.UI.Text mutationRateText;
    public UnityEngine.UI.Text timeText;

    void StopTraining()
    {
        training = false;
    }

    private void Start()
    {
        UnityEngine.Time.timeScale = timeScale;
        astarGrid = GetComponent<Grid>();
        pathfinding = GetComponent<Pathfinding>();
        highestFitness = 0;
        averageFitness = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.Time.timeScale = timeScale;
        if (training == false)
        {
            if (genNumber == 0)
            {
                InitZombies();
            } else
            {
                NeuralNetwork[] bestNets = new NeuralNetwork[4];
                averageFitness = 0;
                for (int l = 0; l < popSize; l++)
                {
                    pathfinding.StartPosition = zombieList[l].transform;
                    astarGrid.StartPosition = zombieList[l].transform;
                    pathfinding.FindPath(zombieList[l].transform.position, goal.transform.position);
                    zombieList[l].distanceFromGoal = pathfinding.FinalPath.Count;
                    float fitnessQuantity = 100;
                    float collisionNum = zombieList[l].noOfCollisions * 2;
                    float relativeDist = 0.0f;
                    if (collisionReward == false)
                    {
                        collisionNum = 1;
                    }
                    if (collisionNum < 1)
                    {
                        collisionNum = 1;
                    }
                    if (zombieList[l].distanceFromGoal > 2)
                    {
                        relativeDist = (zombieList[l].transform.position - goal.transform.position).magnitude / 10;
                        fitnessQuantity = 100000 / zombieList[l].distanceFromGoal;
                    }else
                    {
                        relativeDist = (zombieList[l].transform.position - goal.transform.position).magnitude / 10;
                        fitnessQuantity = 101;
                    }
                    networks[l].SetFitness(fitnessQuantity);
                    averageFitness += fitnessQuantity;
                }
                averageFitness /= popSize;
               // networks.Sort();
                int bestNetIndex = 0;
                highestFitness = networks[popSize-1].GetFitness();
                for (int p = popSize - 1; p > popSize - 6; p--)
                {
                    bestNets[bestNetIndex] = networks[p];
                    bestNetIndex++;
                    if (bestNetIndex == 4)
                    {
                        break;
                    }
                }
                List<NeuralNetwork> nextGen = new List<NeuralNetwork>();
                for (int i = 0; i < popSize-10; i++)
                {
                    //Reproduce with the 'best' solution
                        NeuralNetwork child = RouletteWheelSelection().Reproduction(RouletteWheelSelection());
                        child.MutateAlternate(mutationChance);
                    //networks[i] = child;
                    //networks[i].SetFitness(0.0f);
                    child.SetFitness(0.0f);
                    nextGen.Add(child);

                }
                networks.Sort();
                for (int i = 0; i < popSize-10; i++)
                {
                    networks[i] = nextGen[i];
                }
            }
            genNumber++;
            generationText.text = "Generation Number:" + genNumber.ToString();
            averageFitnessText.text = "Average Fitness:" + averageFitness.ToString();
            timeScaleText.text = "World Time Scale:" + UnityEngine.Time.timeScale.ToString();
            mutationRateText.text = "Mutation Rate:" + mutationChance.ToString() + "%";
            training = true;
            Invoke("StopTraining", trainingTime);
            SpawnZombies();
        }
    }

    private void SpawnZombies()
    {
        if(zombieList != null)
        {
            for (int i = 0; i < zombieList.Count; i++)
            {
                GameObject.Destroy(zombieList[i].gameObject);
            }
        }
        zombieList = new List<ZombieANNScript>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 spawnPos = new Vector3(spawnPoint.x +UnityEngine.Random.Range(-10.0f, 10.0f), spawnPoint.y, spawnPoint.z+UnityEngine.Random.Range(-10.0f, 10.0f));
            Transform spawnTransform = zombie.transform;
            spawnTransform.position = spawnPos;
            ZombieANNScript zombieObj = (Instantiate(zombie, new Vector3(spawnPoint.x + UnityEngine.Random.Range(-10.0f, 10.0f), spawnPoint.y, spawnPoint.z + UnityEngine.Random.Range(-10.0f, 10.0f)),zombie.transform.rotation) as GameObject).GetComponent<ZombieANNScript>();
            zombieObj.Init(networks[i], goal);
            zombieList.Add(zombieObj);
        }
    }

    private void InitZombies()
    {
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


    private NeuralNetwork RouletteWheelSelection()
    {
        float fitnessTotalSum = 0.0f;
        for (int i = 0; i < popSize; i++)
        {
            fitnessTotalSum += networks[i].GetFitness();
        }
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

        return null;
    }
}
