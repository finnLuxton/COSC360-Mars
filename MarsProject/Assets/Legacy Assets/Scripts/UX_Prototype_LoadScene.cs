using UnityEngine;
using UnityEngine.SceneManagement;
 
public class UX_Prototype_LoadScene : MonoBehaviour
{
    public void LoadAnotherScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
