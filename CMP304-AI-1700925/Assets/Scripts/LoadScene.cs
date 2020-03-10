using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Load scene with a name determined by sceneName parameter
public class LoadScene : MonoBehaviour
{
   public void SceneChange(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
