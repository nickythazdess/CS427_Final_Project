using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static void LoadLevel(string name)
    {
        SceneManager.LoadScene(name);
    }
}
