// using UnityEngine;
// using UnityEngine.Audio;
// using System.Collections;

// public class MusicFader : MonoBehaviour
// {
//     [Header("Mixer Settings")]
//     public AudioMixer mixer;
//     public string exposedParameter = "MusicVolume"; // اسم المتغير في الـ Mixer
//     [Header("Fade Settings")]
//     public float fadeTime = 2f; // المدة بالثواني

//     private Coroutine currentFade;

//     void Start()
//     {
//         FadeIn(); // يبدأ Fade In تلقائي عند بداية المشهد
//     Debug.Log("Fading in now...");

//     }

//     public void FadeIn()
//     {
//         if (currentFade != null) StopCoroutine(currentFade);
//         currentFade = StartCoroutine(FadeMixerGroup(-80f, 0f, fadeTime));
//     }

//     public void FadeOut()
//     {
//         if (currentFade != null) StopCoroutine(currentFade);
//         currentFade = StartCoroutine(FadeMixerGroup(0f, -80f, fadeTime));
//     }

//     private IEnumerator FadeMixerGroup(float startVolume, float endVolume, float duration)
//     {
//         float currentTime = 0f;

//         while (currentTime < duration)
//         {
//             currentTime += Time.deltaTime;
//             float newVolume = Mathf.Lerp(startVolume, endVolume, currentTime / duration);
//             mixer.SetFloat(exposedParameter, newVolume);
//             yield return null;
//         }

//         mixer.SetFloat(exposedParameter, endVolume);
//     }
// }


using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class LoopWithFade : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource source;
    public AudioMixer mixer;
    public string exposedParameter = "MusicVolume";

    [Header("Fade Settings")]
    public float fadeInTime = 2f;
    public float fadeOutTime = 2f;
    public float silentGap = 0.5f; // وقت الصمت بين اللوبات

    private bool isPlaying = true;

    void Start()
    {
        StartCoroutine(LoopWithFadeEffect());
    }

    IEnumerator LoopWithFadeEffect()
    {
        while (isPlaying)
        {
            // --- Fade In ---
            source.Play();
            yield return StartCoroutine(FadeMixerGroup(-80f, 0f, fadeInTime));

            // --- Play for most of the clip ---
            yield return new WaitForSeconds(source.clip.length - (fadeInTime + fadeOutTime));

            // --- Fade Out ---
            yield return StartCoroutine(FadeMixerGroup(0f, -80f, fadeOutTime));

            source.Stop();
            yield return new WaitForSeconds(silentGap);
        }
    }

    IEnumerator FadeMixerGroup(float start, float end, float duration)
    {
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(start, end, currentTime / duration);
            mixer.SetFloat(exposedParameter, newVolume);
            yield return null;
        }
        mixer.SetFloat(exposedParameter, end);
    }
}
