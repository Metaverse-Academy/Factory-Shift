using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneNight1Transition : MonoBehaviour
{
    [Header("Night 1 Card")]
    public CanvasGroup night1CanvasGroup;      // Night 1 panel in the CUTSCENE scene
    public float fadeInDuration = 1.0f;
    public float holdDuration = 1.0f;

    [Header("Next Scene")]
    public string night1SceneName = "Night1Scene"; // <-- set to your real Night 1 scene name

    private bool isRunning = false;

    private void Start()
    {
        if (night1CanvasGroup != null)
        {
            if (!night1CanvasGroup.gameObject.activeSelf)
                night1CanvasGroup.gameObject.SetActive(true);

            night1CanvasGroup.alpha = 0f; // start invisible
        }
    }

    // ✅ Call this when the cutscene ends
    public void StartNight1Transition()
    {
        if (isRunning) return;
        StartCoroutine(Night1Sequence());
    }

    private IEnumerator Night1Sequence()
    {
        isRunning = true;
        Time.timeScale = 1f;

        if (night1CanvasGroup != null)
        {
            // Fade IN Night 1
            float t = 0f;
            night1CanvasGroup.alpha = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                night1CanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
                yield return null;
            }
            night1CanvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSeconds(holdDuration);
        }

        // Load Night 1 scene while Night 1 card is fully ON
        if (!string.IsNullOrEmpty(night1SceneName))
        {
            SceneManager.LoadScene(night1SceneName);
        }
        else
        {
            Debug.LogError("CutsceneNight1Transition: night1SceneName is empty!");
        }

        isRunning = false;
    }
}
