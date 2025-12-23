using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameMUSetting : MonoBehaviour
{
    void Awake()
    {
        SceneManager.LoadScene("UIScene", LoadSceneMode.Additive);
    }
    public void LoginTo()
    {
        SceneManager.LoadScene("Login");
    }
}
