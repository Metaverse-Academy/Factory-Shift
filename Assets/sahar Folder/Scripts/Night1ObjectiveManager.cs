using UnityEngine;

public class Night1ObjectiveManager : MonoBehaviour
{
    [Header("Fans to repair")]
    [SerializeField] private RepairableFixable[] fans;

    [Header("Objective UI Texts")]
    [TextArea] [SerializeField] private string readNoteText    = "Read the note";
    [TextArea] [SerializeField] private string goToLadderText  = "Go to the vents ladder";
    [TextArea] [SerializeField] private string repairFansText  = "Repair both fans";
    [TextArea] [SerializeField] private string exitVentsText   = "Exit the vents";
    [TextArea] [SerializeField] private string goHomeText      = "Go Home";

    [Header("Objective UI")]
    [SerializeField] private ObjectiveUI objectiveUI;

    [Header("Highlights")]
    [Tooltip("Highlight the NOTE first.")]
    [SerializeField] private ObjectStateController noteHighlight;

    [Tooltip("Highlight the ladder AFTER the note is read.")]
    [SerializeField] private ObjectStateController ladderHighlight;

    [Tooltip("Highlight these while repairing fans.")]
    [SerializeField] private ObjectStateController[] fanHighlights;

    [Tooltip("Highlight this exit square INSIDE vents after fans repaired.")]
    [SerializeField] private ObjectStateController ventExitHighlight;

    [Tooltip("Highlight the MAIN DOOR AFTER exiting vents.")]
    [SerializeField] private ObjectStateController mainDoorHighlight;

    private int repairedCount;

    private enum Step
    {
        ReadNote,
        GoToLadder,
        RepairFans,
        ExitVents,
        GoHome,
        Done
    }

    private Step currentStep;

    private void Start()
    {
        repairedCount = 0;
        currentStep = Step.ReadNote;

        // UI -> first objective
        if (objectiveUI != null)
            objectiveUI.SetInitial(readNoteText);

        // Highlights start state
        SetHighlight(noteHighlight, true);
        SetHighlight(ladderHighlight, false);
        SetFanHighlights(false);
        SetHighlight(ventExitHighlight, false);
        SetHighlight(mainDoorHighlight, false);

        // Subscribe to fans
        foreach (var fan in fans)
        {
            if (fan == null) continue;

            if (fan.IsRepaired) repairedCount++;

            fan.OnRepaired += HandleFanRepaired;
        }

        // If already repaired in editor (rare, but safe)
        if (repairedCount >= fans.Length && fans.Length > 0)
            OnAllFansRepaired();
    }

    private void OnDestroy()
    {
        foreach (var fan in fans)
        {
            if (fan == null) continue;
            fan.OnRepaired -= HandleFanRepaired;
        }
    }

    // =====================
    // NOTE STEP
    // =====================
    public void OnNoteRead()
    {
        if (currentStep != Step.ReadNote) return;

        currentStep = Step.GoToLadder;

        if (objectiveUI != null)
            objectiveUI.CompleteAndShowNext(goToLadderText);

        // switch highlights:
        SetHighlight(noteHighlight, false);
        SetHighlight(ladderHighlight, true);
    }

    // =====================
    // FAN STEP
    // =====================
    private void HandleFanRepaired(RepairableFixable fan)
    {
        if (currentStep != Step.RepairFans) return;

        repairedCount++;
        Debug.Log($"Fans repaired: {repairedCount}/{fans.Length}");

        if (repairedCount >= fans.Length)
            OnAllFansRepaired();
        else
        {
            // optional UI count
            if (objectiveUI != null)
                objectiveUI.SetInitial($"{repairFansText} ({repairedCount}/{fans.Length})");
        }
    }

    private void OnAllFansRepaired()
    {
        currentStep = Step.ExitVents;

        if (objectiveUI != null)
            objectiveUI.CompleteAndShowNext(exitVentsText);

        // switch highlights:
        SetFanHighlights(false);
        SetHighlight(ventExitHighlight, true);
    }

    // =====================
    // Called by portals
    // =====================

    // ladder entry portal -> player teleports INTO vents
    public void OnEnterVents()
    {
        if (currentStep != Step.GoToLadder) return;

        currentStep = Step.RepairFans;

        if (objectiveUI != null)
            objectiveUI.CompleteAndShowNext(repairFansText);

        // switch highlights:
        SetHighlight(ladderHighlight, false);
        SetFanHighlights(true);
    }

    // vent exit square portal -> player teleports OUT to map
    public void OnExitVents()
    {
        if (currentStep != Step.ExitVents) return;

        currentStep = Step.GoHome;

        if (objectiveUI != null)
            objectiveUI.CompleteAndShowNext(goHomeText);

        // switch highlights:
        SetHighlight(ventExitHighlight, false);
        SetHighlight(mainDoorHighlight, true);
    }

    // ---------------- Highlight helpers ----------------
    private void SetFanHighlights(bool on)
    {
        if (fanHighlights == null) return;
        foreach (var h in fanHighlights)
            if (h != null) h.SetHighlightActive(on);
    }

    private void SetHighlight(ObjectStateController h, bool on)
    {
        if (h != null) h.SetHighlightActive(on);
    }
}
