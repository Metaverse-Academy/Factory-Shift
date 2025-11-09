using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class RepairableFixable : MonoBehaviour
{
    [Header("Who can interact")]
    [SerializeField] private string playerTag = "Player";

    [Header("Input (New Input System)")]
    [Tooltip("Bind to F (Hold) in your actions asset.")]
    [SerializeField] private InputActionReference holdFixAction; // F
    [Tooltip("Key to hit during skill-check (e.g., Space).")]
    [SerializeField] private Key skillCheckKey = Key.Space;

    [Header("UI: Prompt")]
    [SerializeField] private GameObject promptRoot;   // "HOLD F TO FIX"
    [SerializeField] private TMP_Text promptLabel;    // optional
    [SerializeField] private string promptText = "HOLD F TO FIX";

    [Header("UI: Hold Progress")]
    [SerializeField] private CanvasGroup holdGroup;   // show while holding F
    [Tooltip("Radial Image (Fill Method = Radial 360).")]
    [SerializeField] private Image holdFill;          // fillAmount 0..1

    [Header("Repair Settings")]
    [SerializeField] private float totalRepairSeconds = 6.0f;
    [SerializeField] private bool pauseWhenReleased = true;
    [SerializeField] private float decayPerSecond = 0f;

    // -------------------- Circular Skill Check --------------------
    [Header("DBD-like Skill Check (Circular)")]
    [SerializeField] private CanvasGroup skillGroup;    // root group for the skill-check UI
    [SerializeField] private RectTransform needle;      // rotates around Z (pivot at bottom-center)
    [SerializeField] private Image successSlice;        // wedge/arc image (purely visual)
    [Tooltip("Center of success zone in CLOCK degrees: 0=up, 90=right, 180=down, 270=left.")]
    [SerializeField] private float successCenterDeg = 300f;
    [Tooltip("Arc width of success zone in degrees.")]
    [SerializeField] private float successArcDeg = 30f;
    [Tooltip("Needle rotation speed (deg/sec).")]
    [SerializeField] private float needleSpeedDegPerSec = 360f;
    [Tooltip("Random time range between skill checks while holding (seconds).")]
    [SerializeField] private Vector2 skillCheckEverySeconds = new Vector2(2.5f, 5.0f);
    [Tooltip("Time window (sec) after entering zone before leaving counts as a miss.")]
    [SerializeField] private float pressGraceSeconds = 0.15f;
    [Tooltip("Repair bonus/penalty on success/fail (0..1 of total).")]
    [SerializeField] private float successBonus = 0.12f;
    [SerializeField] private float failPenalty = 0.18f;
    [SerializeField] private bool useUnscaledTimeForSkill = true;
    // --- Objective UI (checkmark) ---
    [Header("Objective UI")]
    [SerializeField] private CanvasGroup objectiveCheck; // CanvasGroup on the check sprite
    [SerializeField] private float checkFadeDuration = 0.35f;


    private const float CLOCK_ZERO_IS_UP = 90f; // converts Unity's 0°=right to 0°=up

    [Header("Finish")]
    [SerializeField] private GameObject[] enableOnComplete;
    [SerializeField] private GameObject[] disableOnComplete;

    // state
    private bool playerInRange;
    private bool holding;
    private float progress01; // 0..1
    private float nextSkillCheckTime; // world time for next skill-check
    private bool skillActive;
    private float needleAngleClock; // 0..360 in CLOCK convention (0=up, cw+)
    private float enteredZoneTime = -999f;
    private bool repaired;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnEnable()
    {
        if (objectiveCheck)
        {
            if (!objectiveCheck.gameObject.activeSelf) objectiveCheck.gameObject.SetActive(true);
            objectiveCheck.alpha = 0f; // keep hidden until we finish
        }

        if (promptLabel) promptLabel.text = promptText;
        ShowPrompt(false);
        ShowHold(false);
        ShowSkill(false);

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
        CancelSkillCheck();
    }

    private void Update()
    {
        if (repaired) return;

        bool press = playerInRange && holdFixAction != null && holdFixAction.action.IsPressed();

        if (press && !holding)
        {
            holding = true;
            ShowPrompt(false);
            ShowHold(true);
            ScheduleNextSkillCheck();
        }
        else if (!press && holding)
        {
            holding = false;
        }

        if (holding)
        {
            float perSec = 1f / Mathf.Max(0.01f, totalRepairSeconds);
            progress01 = Mathf.Clamp01(progress01 + perSec * Time.deltaTime);
        }
        else if (!pauseWhenReleased && progress01 > 0f)
        {
            progress01 = Mathf.Clamp01(progress01 - decayPerSecond * Time.deltaTime);
        }

        if (holdFill) holdFill.fillAmount = progress01;

        if (holding) HandleSkillChecks();
        else CancelSkillCheck();

        if (progress01 >= 1f && !repaired)
        {
            repaired = true;
            CompleteRepair();
        }
    }

    // ------------ UI helpers ------------
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

    private void ShowSkill(bool on)
    {
        if (!skillGroup) return;
        skillGroup.alpha = on ? 1f : 0f;
        skillGroup.gameObject.SetActive(on);
    }

    // ------------ Skill-check logic ------------
    private void ScheduleNextSkillCheck()
    {
        nextSkillCheckTime = Time.time + Random.Range(skillCheckEverySeconds.x, skillCheckEverySeconds.y);
    }

    private void HandleSkillChecks()
    {
        if (!skillActive && Time.time >= nextSkillCheckTime)
            StartSkillCheck();

        if (!skillActive) return;

        float dt = useUnscaledTimeForSkill ? Time.unscaledDeltaTime : Time.deltaTime;

        // advance in CLOCK space (0=up, cw+)
        needleAngleClock = Mathf.Repeat(needleAngleClock + needleSpeedDegPerSec * dt, 360f);

        // apply to UI (convert clock→UI: zEuler = -(clock - 90))
        if (needle)
            needle.localRotation = Quaternion.Euler(0f, 0f, -(needleAngleClock - CLOCK_ZERO_IS_UP));

        // space key press
        if (Keyboard.current != null && Keyboard.current[skillCheckKey].wasPressedThisFrame)
        {
            if (IsNeedleInSuccess())
            {
                progress01 = Mathf.Clamp01(progress01 + successBonus);
                EndSkillCheck();
                ScheduleNextSkillCheck();
            }
            else
            {
                progress01 = Mathf.Clamp01(progress01 - failPenalty);
                EndSkillCheck();
                nextSkillCheckTime = Time.time + 1.25f;
            }
        }

        // grace-based miss (left zone without pressing)
        if (enteredZoneTime > 0f && (useUnscaledTimeForSkill ? Time.unscaledTime : Time.time) - enteredZoneTime > pressGraceSeconds && !IsNeedleInSuccess())
        {
            progress01 = Mathf.Clamp01(progress01 - failPenalty);
            EndSkillCheck();
            nextSkillCheckTime = Time.time + 1.25f;
        }

        // track entering zone
        if (IsNeedleInSuccess())
        {
            if (enteredZoneTime < 0f)
                enteredZoneTime = useUnscaledTimeForSkill ? Time.unscaledTime : Time.time;
        }
    }

    private void StartSkillCheck()
    {
        skillActive = true;
        enteredZoneTime = -999f;

        // ensure pivots & anchors so the needle spins like a clock hand
        SetupDial();

        ShowSkill(true);

        // randomize starting angle & spin direction in CLOCK space
        needleAngleClock = Random.Range(0f, 360f);
        if (Random.value < 0.5f) needleSpeedDegPerSec = -Mathf.Abs(needleSpeedDegPerSec);
        else needleSpeedDegPerSec = Mathf.Abs(needleSpeedDegPerSec);

        // place/rotate success wedge in CLOCK convention
        if (successSlice)
            successSlice.rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, -(successCenterDeg - CLOCK_ZERO_IS_UP));
    }

    private void EndSkillCheck()
    {
        skillActive = false;
        enteredZoneTime = -999f;
        ShowSkill(false);
    }

    private void CancelSkillCheck()
    {
        if (!skillActive) return;
        EndSkillCheck();
    }

    private bool IsNeedleInSuccess()
    {
        // needleAngleClock is already in 0..360 with 0=up (clockwise positive)
        float half = successArcDeg * 0.5f;
        float delta = Mathf.DeltaAngle(successCenterDeg, needleAngleClock); // -180..180
        return Mathf.Abs(delta) <= half;
    }

    private void SetupDial()
    {
        // Needle rotates around its base at the dial center
        if (needle != null)
        {
            needle.pivot = new Vector2(0.5f, 0f);
            needle.anchorMin = needle.anchorMax = new Vector2(0.5f, 0.5f);
            needle.anchoredPosition = Vector2.zero;
        }

        if (successSlice != null)
        {
            var rt = successSlice.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            // For a perfect visual match set Image:
            // - Type: Filled
            // - Fill Method: Radial 360
            // - Fill Origin: Top
            // - Clockwise: ON
            // and control its localRotation only from code above.
        }
    }

    // ------------ Finish ------------
    private void CompleteRepair()
    {
        ShowPrompt(false);
        ShowHold(false);
        CancelSkillCheck();

        foreach (var go in enableOnComplete) if (go) go.SetActive(true);
        foreach (var go in disableOnComplete) if (go) go.SetActive(false);

        // Fade in the objective checkmark
        if (objectiveCheck) StartCoroutine(FadeIn(objectiveCheck, checkFadeDuration));
    }

    private System.Collections.IEnumerator FadeIn(CanvasGroup cg, float duration)
    {
        if (!cg) yield break;
        if (!cg.gameObject.activeSelf) cg.gameObject.SetActive(true);

        float t = 0f;
        cg.alpha = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;   // unscaled so it still fades if you pause
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;

    }
}



