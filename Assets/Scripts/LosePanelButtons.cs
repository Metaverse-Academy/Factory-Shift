using UnityEngine;
using UnityEngine.SceneManagement;

public class LosePanelButtons : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("The scene you want to load when pressing Restart (your main gameplay scene).")]
    public string gameSceneName = "YourGameSceneName";

    [Tooltip("The scene you want to load when pressing Main Menu.")]
    public string mainMenuSceneName = "MainMenu";

    public void RestartGame()
    {
        Time.timeScale = 1f; // make sure time is normal again
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("LosePanelButtons: gameSceneName is empty!");
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // unpause
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("LosePanelButtons: mainMenuSceneName is empty!");
        }
    }
}
