using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieANNScript : MonoBehaviour
{
    //Public parameters
    public GameObject goal;
    public float[] annResult;
    NeuralNetwork ann;
    private int[] layers = new int[] { 2, 4, 4, 4 };
    // Start is called before the first frame update
    void Start()
    {
        ann = new NeuralNetwork(layers);
        ann.Mutate();
    }

    private void FixedUpdate()
    {
        float[] inputs =  new float[2];
        inputs[0] = (float)goal.transform.position.x;
        inputs[1] =(float) goal.transform.position.y;
        annResult = ann.ForwardProp(inputs);
    }
}
