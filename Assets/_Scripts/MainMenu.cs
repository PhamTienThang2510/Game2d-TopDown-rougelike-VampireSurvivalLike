using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Pick");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void GoToArena()
    {
        SceneManager.LoadScene("Arena");
    }
    public void GoToOpenWorld()
    {
        SceneManager.LoadScene("OpenWorld");
    }
}
