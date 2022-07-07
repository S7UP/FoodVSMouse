using UnityEngine;
using UnityEngine.SceneManagement;
public class StartScene : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        string startSceneName = "StartLoadScene";
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name.Equals(startSceneName))
        {
            return;
        }
        SceneManager.LoadScene(startSceneName);
    }


}