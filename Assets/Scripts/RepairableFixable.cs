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



private const float CLOCK_ZERO_IS_UP = 90f; // converts Unity's 0°=right to 0°=up
    // ---- Skill-check feedback ----
[Header("Skill Check Feedback")]
[SerializeField] private AudioSource sfxSource;    // success only
[SerializeField] private AudioClip successClip;

[SerializeField, Range(0f, 1f)] private float successVolume = 1f;
[SerializeField] private AudioClip failClip;
[SerializeField, Range(0f, 1f)] private float failVolume = 1f;
// ===== Repair SFX (loop while repairing) =====
[Header("Repair Loop SFX")]
[SerializeField] private AudioSource repairSource;   // add an AudioSource, Play On Awake OFF
[SerializeField] private AudioClip repairLoop;       // your loop/ambience clip
[SerializeField, Range(0f,1f)] private float repairVolume = 0.8f;
[Header("Repair Complete SFX")]
[SerializeField] private AudioClip repairCompleteClip; // assign your "generator complete" sound
[SerializeField, Range(0f,1f)] private float repairCompleteVolume = 1f;
[SerializeField] private float repairFadeIn = 0.12f;
[SerializeField] private float repairFadeOut = 0.20f;
[Header("Highlights")]
[Tooltip("The vent / ladder highlight that should stop after repair.")]
[SerializeField] private ObjectStateController ventHighlight;

[Tooltip("The door highlight that should turn ON after repair.")]
[SerializeField] private ObjectStateController doorHighlight;


// If true, the loop only plays while the player is actively holding F.
// If false (default), the loop starts the first time they begin repairing and
// continues across pauses until the generator is complete.
[SerializeField] private bool loopOnlyWhileHolding = false;

// internal
private bool repairLoopPlaying;
private bool wasHolding;
public ObjectiveUI objectiveUI;


[SerializeField] private ParticleSystem failExplosionPrefab;

// B) Or drag an existing ParticleSystem in the scene to just Play():
[SerializeField] private ParticleSystem failExplosionInScene;

[Tooltip("Where to place the explosion. If null, uses this object's position.")]
[SerializeField] private Transform explosionSpawnPoint;

[Tooltip("If using a prefab, parent the spawned VFX to this object (so it follows).")]
[SerializeField] private bool attachExplosionToThis = false;

[SerializeField] private CanvasGroup failFlashGroup; // red overlay (alpha 0 by default)
[SerializeField] private float failFlashIn = 0.08f;
[SerializeField] private float failFlashHold = 0.05f;
[SerializeField] private float failFlashOut = 0.20f;
[SerializeField] private bool useUnscaledTimeForFX = true;
[SerializeField] private AudioClip skillAppearClip;
[SerializeField, Range(0f, 1f)] private float skillAppearVolume = 1f;



    [Header("Finish")]
    [SerializeField] private GameObject[] enableOnComplete;
    [SerializeField] private GameObject[] disableOnComplete;

    [Header("Vent Monster on Skill Fail")]
    [SerializeField] private VentMonsterAttack ventMonster;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool triggerMonsterOnFail = true;
    public bool IsRepaired => repaired;
        private bool monsterAlreadyTriggered;


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
          if (failFlashGroup)
    {
        if (!failFlashGroup.gameObject.activeSelf) failFlashGroup.gameObject.SetActive(true);
        failFlashGroup.alpha = 0f;
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
        // Track transitions
bool holdingNow = holding;
if (holdingNow && !wasHolding)
{
    // began holding this frame
    StartRepairLoopIfNeeded();
    if (loopOnlyWhileHolding == true && repairSource && repairLoopPlaying && !repairSource.isPlaying)
        repairSource.Play();
}
else if (!holdingNow && wasHolding)
{
    // released this frame
    if (loopOnlyWhileHolding == true)
        StopRepairLoop(false); // fade out when they stop holding
}
wasHolding = holdingNow;

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
                DoSuccessFeedback(); 
                EndSkillCheck();
                ScheduleNextSkillCheck();
            }
            else
            {
                progress01 = Mathf.Clamp01(progress01 - failPenalty);
                DoFailFeedback();
                PlayFailSfx();
                TriggerFailExplosion();
                EndSkillCheck();
                nextSkillCheckTime = Time.time + 1.25f;
            }
        }

        // grace-based miss (left zone without pressing)
        if (enteredZoneTime > 0f && (useUnscaledTimeForSkill ? Time.unscaledTime : Time.time) - enteredZoneTime > pressGraceSeconds && !IsNeedleInSuccess())
        {
            progress01 = Mathf.Clamp01(progress01 - failPenalty);
            DoFailFeedback();  
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

    // make sure pivots/anchors are correct so it rotates like a clock hand
    SetupDial();

    ShowSkill(true);
    PlaySkillAppearSfx(); 

    // ALWAYS start at the top (0° = up in our clock convention)
    needleAngleClock = 0f;

    // ALWAYS rotate clockwise
    needleSpeedDegPerSec = Mathf.Abs(needleSpeedDegPerSec);

    // Place/rotate the success wedge in CLOCK convention (0=up)
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
           
        }
    }

       private void CompleteRepair()
    {
        ShowPrompt(false);
        ShowHold(false);
        CancelSkillCheck();
        StopRepairLoop(false);
        PlayRepairCompleteSfx(); 

        // 👉 Update objective text
        if (objectiveUI != null)
        {
            objectiveUI.CompleteAndShowNext("Go Home");
        }

        // 👉 Switch highlights:
        // turn OFF vent highlight
        if (ventHighlight != null)
            ventHighlight.SetHighlightActive(false);

        // turn ON door highlight
        if (doorHighlight != null)
            doorHighlight.SetHighlightActive(true);

        // Enable / disable world objects as before
        foreach (var go in enableOnComplete) 
            if (go) go.SetActive(true);

        foreach (var go in disableOnComplete) 
            if (go) go.SetActive(false);
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
    private void PlaySuccessSfx()
    {
        if (successClip == null) return;

        if (sfxSource != null)
            sfxSource.PlayOneShot(successClip, successVolume);
        else
            AudioSource.PlayClipAtPoint(successClip, transform.position, successVolume);
    }
private void PlayRepairCompleteSfx()
{
    if (repairCompleteClip == null) return;

    if (sfxSource != null)
        sfxSource.PlayOneShot(repairCompleteClip, repairCompleteVolume);
    else
        AudioSource.PlayClipAtPoint(repairCompleteClip, transform.position, repairCompleteVolume);
}



private System.Collections.IEnumerator FlashFailRed()
{
    if (!failFlashGroup) yield break;

    float dt() => useUnscaledTimeForFX ? Time.unscaledDeltaTime : Time.deltaTime;

    // Fade in
    failFlashGroup.alpha = 0f;
    float t = 0f;
    while (t < failFlashIn)
    {
        t += dt();
        failFlashGroup.alpha = Mathf.Lerp(0f, 1f, t / failFlashIn);
        yield return null;
    }
    failFlashGroup.alpha = 1f;

    // Hold
    t = 0f;
    while (t < failFlashHold)
    {
        t += dt();
        yield return null;
    }

    // Fade out
    t = 0f;
    while (t < failFlashOut)
    {
        t += dt();
        failFlashGroup.alpha = Mathf.Lerp(1f, 0f, t / failFlashOut);
        yield return null;
    }
    failFlashGroup.alpha = 0f;
}

private void DoFailFeedback()
{
    StartCoroutine(FlashFailRed());
    PlayFailSfx();
    TriggerFailExplosion();

    // Call vent monster here
    TriggerVentMonster();
}


    private void DoSuccessFeedback()
    {
        // Sound only
        PlaySuccessSfx();
    }
   private void PlayFailSfx()
{
    if (failClip == null) return;

    if (sfxSource != null)
        sfxSource.PlayOneShot(failClip, failVolume);
    else
        AudioSource.PlayClipAtPoint(failClip, transform.position, failVolume);
}

private void TriggerFailExplosion()
{
    // Priority: in-scene system (just play it) -> prefab (instantiate)
    if (failExplosionInScene != null)
    {
        var pos = explosionSpawnPoint ? explosionSpawnPoint.position : transform.position;
        failExplosionInScene.transform.position = pos;
        // Optional: match rotation
        if (explosionSpawnPoint) failExplosionInScene.transform.rotation = explosionSpawnPoint.rotation;
        failExplosionInScene.Play(true);
        return;
    }

    if (failExplosionPrefab != null)
    {
        var pos = explosionSpawnPoint ? explosionSpawnPoint.position : transform.position;
        var rot = explosionSpawnPoint ? explosionSpawnPoint.rotation : Quaternion.identity;
        var parent = attachExplosionToThis ? transform : null;

        var ps = Instantiate(failExplosionPrefab, pos, rot, parent);
        ps.Play(true);

        // Clean up once finished (safe lifetime calculation)
        var main = ps.main;
        float killAfter = main.duration + main.startLifetime.constantMax + 0.5f;
        Destroy(ps.gameObject, killAfter);
    }
}
    private void TriggerVentMonster()
    {
        if (!triggerMonsterOnFail) return;
        if (monsterAlreadyTriggered) return;

        monsterAlreadyTriggered = true;

        if (ventMonster != null && playerTransform != null)
        {
            ventMonster.StartVentAttack(playerTransform);
        }
        else
        {
            Debug.LogWarning("RepairableFixable: Vent monster or playerTransform not assigned.");
        }
    }



    private void PlaySkillAppearSfx()
    {
        if (skillAppearClip == null) return;

        if (sfxSource != null)
            sfxSource.PlayOneShot(skillAppearClip, skillAppearVolume);
        else
            AudioSource.PlayClipAtPoint(skillAppearClip, transform.position, skillAppearVolume);
    }
private System.Collections.IEnumerator FadeVolume(AudioSource src, float from, float to, float duration, bool stopAtEnd)
{
    if (!src) yield break;
    float t = 0f;
    src.volume = from;
    while (t < duration)
    {
        t += Time.unscaledDeltaTime;
        src.volume = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
        yield return null;
    }
    src.volume = to;
    if (stopAtEnd && Mathf.Approximately(to, 0f)) src.Stop();
}

private void StartRepairLoopIfNeeded()
{
    if (!repairSource || !repairLoop || repairLoopPlaying) return;

    repairSource.loop = true;
    repairSource.clip = repairLoop;
    repairSource.volume = 0f;
    repairSource.Play();
    StartCoroutine(FadeVolume(repairSource, 0f, repairVolume, repairFadeIn, false));
    repairLoopPlaying = true;
}

private void StopRepairLoop(bool immediate = false)
{
    if (!repairSource || !repairLoopPlaying) return;

    if (immediate || repairFadeOut <= 0f)
    {
        repairSource.Stop();
        repairSource.volume = 0f;
    }
    else
    {
        StartCoroutine(FadeVolume(repairSource, repairSource.volume, 0f, repairFadeOut, true));
    }
    repairLoopPlaying = false;
}




}



