using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneAutoReturn : MonoBehaviour
{
    [Header("Return")]
    [Tooltip("Leave empty to return to the scene you came from.")]
    [SerializeField] private string returnSceneOverride = "";

    [Header("Outro Overlay (UI)")]
    [Tooltip("CanvasGroup on the overlay root that contains the image and text.")]
    [SerializeField] private CanvasGroup outroGroup;      // drag your OutroOverlay (CanvasGroup)
    [SerializeField] private TMP_Text outroText;          // drag the centered TMP text
    [TextArea] [SerializeField] private string message = "Mission Updated";

    [Header("Timings (seconds)")]
    [SerializeField] private float fadeIn = 0.6f;
    [SerializeField] private float hold   = 1.2f;
    [SerializeField] private float fadeOut= 0.6f;

    [Header("Options")]
    [SerializeField] private bool useUnscaledTime = true; // unaffected by timescale

    private PlayableDirector director;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += OnCutsceneStopped;

        if (outroGroup) outroGroup.alpha = 0f; // start hidden
        if (outroText)  outroText.text = message;
    }

    private void OnDestroy()
    {
        if (director) director.stopped -= OnCutsceneStopped;
    }

    private void Start()
    {
        director.time = 0;
        director.Play();
    }

    private void OnCutsceneStopped(PlayableDirector _)
    {
        string target = string.IsNullOrEmpty(CutsceneRoute.ReturnSceneName) ? "gameplay" : CutsceneRoute.ReturnSceneName;
    GlobalScreenFader.Instance.LoadSceneWithFade(target, 0.6f, 0.6f);
    }

    private System.Collections.IEnumerator DoOutroAndReturn()
    {
        if (outroGroup)
        {
            // Fade in
            yield return FadeCanvasGroup(outroGroup, 0f, 1f, fadeIn);
            // Hold
            yield return Wait(hold);
            // Fade out
            yield return FadeCanvasGroup(outroGroup, 1f, 0f, fadeOut);
        }

        string target = !string.IsNullOrEmpty(returnSceneOverride)
            ? returnSceneOverride
            : (string.IsNullOrEmpty(CutsceneRoute.ReturnSceneName) ? "YourGameSceneName" : CutsceneRoute.ReturnSceneName);

        SceneManager.LoadScene(target, LoadSceneMode.Single);
    }

    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (duration <= 0f) { cg.alpha = to; yield break; }

        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private System.Collections.IEnumerator Wait(float seconds)
    {
        if (seconds <= 0f) yield break;
        float t = 0f;
        while (t < seconds)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }
    
}
