using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class RepairableFixable : MonoBehaviour
{
    [Header("Who can interact")]
    [SerializeField] private string playerTag = "Player";

    [Header("Input (New Input System)")]
    [Tooltip("Bind this action to F (Hold) in your Input Actions asset.")]
    [SerializeField] private InputActionReference holdFixAction; // F

    [Header("UI: Prompt")]
    [SerializeField] private GameObject promptRoot;  // "HOLD F TO FIX"
    [SerializeField] private TMP_Text promptLabel;   // optional
    [SerializeField] private string promptText = "HOLD F TO FIX";

    [Header("UI: Hold Progress")]
    [SerializeField] private CanvasGroup holdGroup;  // shows while holding
    [Tooltip("Radial Image (Image Type = Filled, Fill Method = Radial 360).")]
    [SerializeField] private Image holdFill;         // fillAmount 0..1

    [Header("Repair Settings")]
    [Tooltip("Seconds required to finish (holding continuously).")]
    [SerializeField] private float totalRepairSeconds = 6.0f;
    [Tooltip("If OFF, progress decays when you release F.")]
    [SerializeField] private bool pauseWhenReleased = true;
    [SerializeField] private float decayPerSecond = 0.5f;

    [Header("On Complete (optional)")]
    [SerializeField] private GameObject[] enableOnComplete;
    [SerializeField] private GameObject[] disableOnComplete;
    [SerializeField] private UnityEvent onRepairCompleted; // hook SFX/VFX

    // state
    private bool playerInRange;
    private bool holding;
    private bool repaired;
    private float progress01; // 0..1

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnEnable()
    {
        if (promptLabel) promptLabel.text = promptText;
        ShowPrompt(false);
        ShowHold(false);

        if (holdFixAction != null)
            holdFixAction.action.Enable();
    }

    private void OnDisable()
    {
        if (holdFixAction != null)
            holdFixAction.action.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (repaired) return;
        if (!other.CompareTag(playerTag)) return;

        playerInRange = true;
        ShowPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        holding = false;
        ShowPrompt(false);
        ShowHold(false);
    }

    private void Update()
    {
        if (repaired) return;

        // 1) Check hold
        bool isPressed = playerInRange && holdFixAction != null && holdFixAction.action.IsPressed();

        if (isPressed && !holding)
        {
            holding = true;
            ShowPrompt(false);
            ShowHold(true);
        }
        else if (!isPressed && holding)
        {
            holding = false;
            if (!pauseWhenReleased && progress01 > 0f)
            {
                // will decay below
            }
            else if (pauseWhenReleased && progress01 <= 0f)
            {
                ShowHold(false);
                ShowPrompt(true);
            }
        }

        // 2) Progress / decay
        if (holding)
        {
            float perSec = 1f / Mathf.Max(0.01f, totalRepairSeconds);
            progress01 = Mathf.Clamp01(progress01 + perSec * Time.deltaTime);
        }
        else if (!pauseWhenReleased && progress01 > 0f)
        {
            progress01 = Mathf.Clamp01(progress01 - decayPerSecond * Time.deltaTime);
            if (progress01 <= 0f)
            {
                ShowHold(false);
                if (playerInRange) ShowPrompt(true);
            }
        }

        // 3) Update UI
        if (holdFill) holdFill.fillAmount = progress01;

        // 4) Finish
        if (progress01 >= 1f && !repaired)
        {
            repaired = true;
            CompleteRepair();
        }
    }

    private void CompleteRepair()
    {
        ShowPrompt(false);
        ShowHold(false);

        foreach (var go in enableOnComplete) if (go) go.SetActive(true);
        foreach (var go in disableOnComplete) if (go) go.SetActive(false);

        onRepairCompleted?.Invoke();
    }

    // ---- UI helpers ----
    private void ShowPrompt(bool on)
    {
        if (promptRoot) promptRoot.SetActive(on);
    }

    private void ShowHold(bool on)
    {
        if (!holdGroup) return;
        holdGroup.alpha = on ? 1f : 0f;
        holdGroup.gameObject.SetActive(on);
    }
}
