using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void StartGame()
    {
        // Load the game scene (assuming it's named "GameScene")
        SceneManager.LoadScene("PangGame");
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }

}
