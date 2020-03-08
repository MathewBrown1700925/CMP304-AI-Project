using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public bool training = false;
    public int popSize = 50;
    [Range(1,100)]
    public int timeScale = 1;
    public GameObject goal;
    public float closestPos;
    public int genNumber = 0;
    private int[] layers = new int[] { 2, 3, 3, 4 };
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
        if(training == false)
        {
            if (genNumber == 0)
            {
                InitZombies();
            } else
            {
                closestPos = Mathf.Infinity;
                int closestIndex = -1;
                for (int l = 0; l < popSize; l++)
                {
                    if ((zombieList[l].gameObject.transform.position - goal.transform.position).magnitude < closestPos)
                    {
                        closestPos = (zombieList[l].gameObject.transform.position - goal.transform.position).magnitude;
                        closestIndex = l;
                    }
                }
                networks[closestIndex].IncrimentFitness(1);
                networks.Sort();
                for (int i = 0; i < popSize-1; i++)
                {
                    networks[i] = new NeuralNetwork(networks[popSize-1]);
                    networks[i].Mutate();
                    networks[popSize-1] = new NeuralNetwork(networks[popSize-1]);
                }
                for (int i = 0; i < popSize; i++)
                {
                    networks[i].SetFitness(0.0f);
                }
            }
            genNumber++;
            training = true;
            Invoke("StopTraining", 15f);
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

            ZombieANNScript zombieObj = ((GameObject)Instantiate(zombie, new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f),0),zombie.transform.rotation)).GetComponent<ZombieANNScript>();
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
