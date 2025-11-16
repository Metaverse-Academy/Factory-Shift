using System.Collections;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    private void Reset()
    {
        fadeGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (!fadeGroup) return;
        fadeGroup.alpha = 1f;                 // start fully black
        fadeGroup.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            fadeGroup.alpha = 1f - k;
            yield return null;
        }
        fadeGroup.alpha = 0f;
        fadeGroup.gameObject.SetActive(false);
    }
}
