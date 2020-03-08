using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieANNScript : MonoBehaviour
{
    //Public parameters
    public float[] annResult;
    private GameObject goal;
    public int activatedOutputNeuron = -1;
    public float rewardValue = 0.01f;
    NeuralNetwork ann;
    bool initialised = false;
//    private int[] layers = new int[] { 2, 3, 3, 4 };
    // Start is called before the first frame update
    void Start()
    {
       // ann = new NeuralNetwork(layers);
        //ann.Mutate();
    }

    private void FixedUpdate()
    {
        if (initialised == true)
        {
            float[] inputs = new float[2];
            Vector3 relativePos = this.transform.position - goal.transform.position;
            inputs[0] = (float)relativePos.x;
            inputs[1] = (float)relativePos.z;
            annResult = ann.ForwardProp(inputs);
            activatedOutputNeuron = FindActiveNode(annResult);
            Vector3 newPos = this.transform.position;
            //
            switch (activatedOutputNeuron)
            {
                case 0:
                    newPos.x++;
                    break;
                case 1:
                    newPos.x--;
                    break;
                case 2:
                    newPos.z++;
                    break;
                case 3:
                    newPos.z--;
                    break;
            }
            this.transform.position = newPos;
            //Manual Control
            if (Input.GetKeyDown(KeyCode.A))
            {
                newPos.x--;
                this.transform.position = newPos;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                newPos.x++;
                this.transform.position = newPos;
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                newPos.z++;
                this.transform.position = newPos;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                newPos.z--;
                this.transform.position = newPos;
            }
        }
    }

    private int FindActiveNode(float[] outputs)
    {
        float currentMaxVal = 0.0f;
        int maxIndex = -1;
        for (int i = 0; i < outputs.Length; i++)
        {
            if (outputs[i] > currentMaxVal)
            {
                maxIndex = i;
                currentMaxVal = outputs[i];
            }
        }
        return maxIndex;
    }

    public void Init(NeuralNetwork net,GameObject pGoal)
    {
        ann = net;
        goal = pGoal;
        initialised = true;
    }
}
