using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class GlobalScreenFader : MonoBehaviour
{
    public static GlobalScreenFader Instance { get; private set; }

    [SerializeField] private CanvasGroup cg; // assign the CanvasGroup
    [SerializeField] private bool useUnscaledTime = true;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (!cg) cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
    }

    public IEnumerator FadeTo(float target, float duration)
    {
        if (duration <= 0f) { cg.alpha = target; yield break; }
        float from = cg.alpha, t = 0f;
        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, target, t / duration);
            yield return null;
        }
        cg.alpha = target;
    }

    public void LoadSceneWithFade(string sceneName, float fadeOut = 0.6f, float fadeIn = 0.6f)
    {
        StartCoroutine(CoLoadSceneWithFade(sceneName, fadeOut, fadeIn));
    }

    private IEnumerator CoLoadSceneWithFade(string sceneName, float fadeOut, float fadeIn)
    {
        // Fade to black in current scene (dialogue can finish BEFORE you call this)
        yield return FadeTo(1f, fadeOut);

        // Load while black
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return op;

        // Optional one frame so new scene UI can build
        yield return null;

        // Fade back in on the new scene
        yield return FadeTo(0f, fadeIn);
    }
}
