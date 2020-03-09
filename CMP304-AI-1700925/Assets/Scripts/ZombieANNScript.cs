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
    private float[] obstacleDistance;
    int inputNum = 5;
    public int noOfCollisions = 0;
    float raycastDistance = 10;
    public bool collideFlag = false;
    public bool collideGoal = false;
    NeuralNetwork ann;
    bool initialised = false;
    Rigidbody rigBody;
//    private int[] layers = new int[] { 2, 3, 3, 4 };
    // Start is called before the first frame update
    void Start()
    {
        // ann = new NeuralNetwork(layers);
        //ann.Mutate();
        obstacleDistance = new float[3];
        rigBody = GetComponentInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if ((initialised == true)&&(collideFlag == false))
        {
            float[] inputs = new float[inputNum];
            Vector3 relativePos = this.transform.position - goal.transform.position;
            Ray leftRay = new Ray(this.transform.position, -this.transform.right);
            Ray rightRay = new Ray(this.transform.position, this.transform.right);
            Ray forwardRay = new Ray(this.transform.position, this.transform.forward);
            //
            inputs[0] = (float)relativePos.x;
            inputs[1] = (float)relativePos.z;
            //
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
            annResult = ann.ForwardProp(inputs);
            activatedOutputNeuron = FindActiveNode(annResult);
            Vector3 newPos = this.transform.position;
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
            //this.transform.position = newPos;
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

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.collider.gameObject.tag != "Target")&&(collision.collider.gameObject.tag != "Enviroment"))
        {
            // ann.IncrimentFitness(-1.0f);
            noOfCollisions++;
            collideFlag = true;
        } else if (collision.collider.gameObject.tag == "Target")
        {
            collideGoal = true;
        }
    }
}
