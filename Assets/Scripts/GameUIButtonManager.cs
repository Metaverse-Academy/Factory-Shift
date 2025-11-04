using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIButtonManager : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("The  scene name in Build Settings.")]
    [SerializeField] private string SceneName= "NextLevel";
    [Tooltip("The main menu scene name in Build Settings.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [Tooltip("Optional: The current level scene name (used for restart). Leave empty to reload current scene automatically.")]
    [SerializeField] private string currentLevelSceneName = "";

    // === MAIN MENU BUTTONS ===
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // === WIN / LOSE PANEL BUTTONS ===
    public void RestartLevel()
    {
        if (!string.IsNullOrEmpty(currentLevelSceneName))
            SceneManager.LoadScene(currentLevelSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
