using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Script to handle operations of individual zombie agents
public class ZombieANNScript : MonoBehaviour
{
    //Public Members
    public float[] annResult;
    public int distanceFromGoal = 100;
    public int noOfCollisions = 0;
    //Private Members
    private int activatedOutputNeuron;
    private GameObject goal;
    private float[] obstacleDistance;
    private int inputNum = 5;
    private float raycastDistance = 10;
    private NeuralNetwork ann;
    private bool initialised = false;
    private Rigidbody rigBody;
    //
    void Start()
    {
        //Initialise Upon Creation
        obstacleDistance = new float[3];
        rigBody = GetComponentInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (initialised == true)
        {
            //Set up inputs to neural network and raycasts
            float[] inputs = new float[inputNum];
            Vector3 relativePos = this.transform.position - goal.transform.position;
            Ray leftRay = new Ray(this.transform.position, -this.transform.right);
            Ray rightRay = new Ray(this.transform.position, this.transform.right);
            Ray forwardRay = new Ray(this.transform.position, this.transform.forward);
            //
            inputs[0] = (float)relativePos.x;
            inputs[1] = (float)relativePos.z;
            //
            //Check if objects have been hit with raycast
            //if they have nput the distance at which they were found to the network
            RaycastHit rayDetails = new RaycastHit();
            if (Physics.Raycast(leftRay, out rayDetails,raycastDistance)) {
                inputs[2] = rayDetails.distance;
             }
            //
            if (Physics.Raycast(rightRay, out rayDetails, raycastDistance))
            {
                inputs[3] = rayDetails.distance;
            }
            //
            //
            if (Physics.Raycast(forwardRay, out rayDetails, raycastDistance))
            {
                inputs[4] = rayDetails.distance;
            }
            //
            //Forward Propogation function takes inputs and takes them through layers, outputting the values of all the output nodes
            annResult = ann.ForwardProp(inputs);
            //Find the node for which the network has activated and act according to inputs
            activatedOutputNeuron = FindActiveNode(annResult);
            Vector3 newPos = this.transform.position;
            //Value to determine the magnitude of the force applied to the rigid body components
            float forceVal = 45.0f;
            //
            switch (activatedOutputNeuron)
            {
                case 0:
                    newPos.x++;
                    rigBody.AddForce(new Vector3(1.0f*forceVal, 0.0f, 0.0f));
                    break;
                case 1:
                    newPos.x--;
                    rigBody.AddForce(new Vector3(-1.0f * forceVal, 0.0f, 0.0f));
                    break;
                case 2:
                    newPos.z++;
                    rigBody.AddForce(new Vector3(0.0f, 0.0f, 1.0f * forceVal));
                    break;
                case 3:
                    newPos.z--;
                    rigBody.AddForce(new Vector3(0.0f, 0.0f, -1.0f * forceVal));
                    break;
            }
        }
    }
    //Function to find the active node out of the output layer
    private int FindActiveNode(float[] outputs)
    {
        //Simple Find Max Standard Algorithm
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
    //initialisation function for initialisation after instantiation(update of goal location and neural network)
    public void Init(NeuralNetwork net,GameObject pGoal)
    {
        ann = net;
        goal = pGoal;
        initialised = true;
    }
    //Incriment number of collisions counter when the zombie collides with objects outside of the floor and goal object
    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.collider.gameObject.tag != "Target")&&(collision.collider.gameObject.tag != "Enviroment"))
        {
            noOfCollisions++;
        } 
    }
}
