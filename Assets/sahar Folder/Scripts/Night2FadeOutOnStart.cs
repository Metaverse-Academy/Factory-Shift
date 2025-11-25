using System.Collections;
using UnityEngine;

public class Night2FadeOutOnStart : MonoBehaviour
{
    public CanvasGroup night2CanvasGroup;
    public float fadeOutDuration = 1.0f;

    private void Start()
    {
        if (night2CanvasGroup == null)
            night2CanvasGroup = GetComponent<CanvasGroup>();

        if (night2CanvasGroup != null)
        {
            // تأكد انها ظاهرة بالكامل في البداية
            night2CanvasGroup.alpha = 1f;
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        float startAlpha = night2CanvasGroup.alpha;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, 0f, t / fadeOutDuration);
            night2CanvasGroup.alpha = a;
            yield return null;
        }

        night2CanvasGroup.alpha = 0f;
        night2CanvasGroup.gameObject.SetActive(false); // اختياري: نخفيها بعد الانتهاء
    }
}
