using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class DoorToHome : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Input & UI")]
    [SerializeField] private InputActionReference interactAction; // bind to E
    [SerializeField] private GameObject promptUI;                 // "E to go home"

    [Header("Scene + Fade")]
    [SerializeField] private string targetSceneName = "HomeScene"; // <-- change to your scene name
    [SerializeField] private CanvasGroup fadeGroup;                // full-screen black image
    [SerializeField] private float fadeDuration = 1f;

    private bool inRange;
    private bool isFading;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteract;

        if (promptUI) promptUI.SetActive(false);

        // make sure fade starts transparent in gameplay scene
        if (fadeGroup)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteract;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inRange = true;
        if (promptUI) promptUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inRange = false;
        if (promptUI) promptUI.SetActive(false);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!inRange || isFading) return;
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        if (!fadeGroup)
        {
            // no fade? just load
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }

        isFading = true;
        if (promptUI) promptUI.SetActive(false);

        float t = 0f;
        fadeGroup.gameObject.SetActive(true);

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            fadeGroup.alpha = k;
            yield return null;
        }

        fadeGroup.alpha = 1f;

        SceneManager.LoadScene(targetSceneName);
    }
}
