using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class DoorToHome : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Requirements")]
    [Tooltip("Door only works if ALL of these repairs are completed (optional).")]
    [SerializeField] private RepairableFixable[] requiredRepair;

    [Header("Input & UI")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference gamepadInteractAction;
    [SerializeField] private GameObject promptUI;                 // parent root object

    [Header("Prompt Variants")]
    [Tooltip("Child with keyboard text/icon: 'E to go home'")]
    [SerializeField] private GameObject keyboardPrompt;
    [Tooltip("Child with controller text/icon: 'X / A to go home'")]
    [SerializeField] private GameObject gamepadPrompt;

    [Header("Scene + Fade")]
    [SerializeField] private string targetSceneName = "HomeScene";
    [SerializeField] private CanvasGroup fadeGroup;    // full-screen black image
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
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }
        if (gamepadInteractAction != null)
        {
            gamepadInteractAction.action.Enable();
            gamepadInteractAction.action.performed += OnInteract;
        }

        if (promptUI) promptUI.SetActive(false);

        // fade canvas starts transparent
        if (fadeGroup)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.gameObject.SetActive(true);
        }

        // listen for scheme changes (from InputSchemeUIManager singleton)
        if (InputSchemeUIManager.Instance != null)
        {
            InputSchemeUIManager.Instance.OnSchemeChanged += HandleSchemeChanged;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
        if (gamepadInteractAction != null)
        {
            gamepadInteractAction.action.performed -= OnInteract;
            gamepadInteractAction.action.Disable();
        }

        if (InputSchemeUIManager.Instance != null)
        {
            InputSchemeUIManager.Instance.OnSchemeChanged -= HandleSchemeChanged;
        }
    }

    // 👇 Helper: can the player use this door now?
    private bool CanUseDoor()
    {
        // no requirements → always usable
        if (requiredRepair == null || requiredRepair.Length == 0)
            return true;

        // all required repairs must be complete
        foreach (var repair in requiredRepair)
        {
            if (repair == null) continue;
            if (!repair.IsRepaired) return false;
        }
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inRange = true;

        RefreshPromptVisual();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inRange = false;

        RefreshPromptVisual();
    }

    private void Update()
    {
        // Player might finish repair while standing at door → update prompt
        RefreshPromptVisual();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!inRange || isFading) return;
        if (!CanUseDoor()) return;

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        if (!fadeGroup)
        {
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

    // =======================
    //   UI prompt helpers
    // =======================
    private void RefreshPromptVisual()
    {
        if (!promptUI) return;

        bool shouldShow = inRange && CanUseDoor();

        if (!shouldShow)
        {
            // hide everything
            promptUI.SetActive(false);
            if (keyboardPrompt) keyboardPrompt.SetActive(false);
            if (gamepadPrompt)  gamepadPrompt.SetActive(false);
            return;
        }

        promptUI.SetActive(true);

        bool useGamepad = InputSchemeUIManager.Instance != null &&
                          InputSchemeUIManager.Instance.IsGamepad;

        if (keyboardPrompt) keyboardPrompt.SetActive(!useGamepad);
        if (gamepadPrompt)  gamepadPrompt.SetActive(useGamepad);
    }

    private void HandleSchemeChanged(bool isGamepad)
    {
        // when scheme changes and player is in range, just refresh
        if (inRange)
            RefreshPromptVisual();
    }
}
