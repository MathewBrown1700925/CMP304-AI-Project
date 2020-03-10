using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Call scene load script with a scene determined by sceneName parameter
public class CallLoad : MonoBehaviour
{
    public LoadScene sceneLoad;
    public string sceneName;
    void Start()
    {
        sceneLoad.SceneChange(sceneName);
    }

}
