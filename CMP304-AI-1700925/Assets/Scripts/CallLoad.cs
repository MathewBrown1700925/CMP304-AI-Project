using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallLoad : MonoBehaviour
{
    public LoadScene sceneLoad;
    public string sceneName;
    // Start is called before the first frame update
    void Start()
    {
        sceneLoad.SceneChange(sceneName);
    }

}
