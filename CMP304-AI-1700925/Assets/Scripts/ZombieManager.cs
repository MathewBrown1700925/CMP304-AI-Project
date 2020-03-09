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
    public float closestPos;
    public float furthestPos;
    public int genNumber = 0;
    private int[] layers = new int[] { 5, 4, 4, 4 };
    private List<NeuralNetwork> networks;
    public GameObject zombie;
    private List<ZombieANNScript> zombieList = null;

    void StopTraining()
    {
        training = false;
    }

    private void Start()
    {
        UnityEngine.Time.timeScale = timeScale;
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
                closestPos = Mathf.Infinity;
                for (int l = 0; l < popSize; l++)
                {
                    if (zombieList[l].collideFlag == true)
                    {
                        networks[l].IncrimentFitness(-20);
                    }
                        float relativePos = (zombieList[l].transform.position - goal.transform.position).magnitude;
                        if (relativePos < 1)
                        {
                            relativePos = 1;
                        }
                        float fitnessQuantity = 10 / relativePos + zombieList[l].noOfCollisions*2;
                    if (zombieList[l].collideGoal == true)
                    {
                        networks[l].IncrimentFitness(100);
                    } else if (relativePos < 5)
                    {
                        networks[l].IncrimentFitness(20);
                    } else if (relativePos < 10)
                    {
                        networks[l].IncrimentFitness(10);
                    }
                   
                }
              
                networks.Sort();
                int bestNetIndex = 0;
                for (int p = popSize - 1; p > popSize - 5; p--)
                {
                    bestNets[bestNetIndex] = networks[p];
                    bestNetIndex++;
                    if (bestNetIndex == 4)
                    {
                        break;
                    }
                }
                for (int i = 0; i < popSize; i++)
                {
                    //Reproduce with the 'best' solution
                    NeuralNetwork child = networks[i].Reproduction(bestNets[UnityEngine.Random.Range(0,2)]);
                    child.MutateAlternate(mutationChance);
                    networks[i] = child;
                    networks[i].SetFitness(0.0f);
                }
            }
            genNumber++;
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
            ZombieANNScript zombieObj = ((GameObject)Instantiate(zombie, spawnTransform)).GetComponent<ZombieANNScript>();
            zombieObj.Init(networks[i],goal);
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
}
