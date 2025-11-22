using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ObjectiveUI objectiveUI;

    [TextArea] public string noteText = "Read the note";
    [TextArea] public string fansText = "Repair the fans";
    [TextArea] public string ventText = "Look for the vent";
    [TextArea] public string exitText = "Exit the building";

    [Header("Fans Requirement (set per scene)")]
    [Tooltip("How many fans must be repaired in this scene to advance.")]
    [SerializeField] private int fansToRepair = 1;

    [Header("Highlight Targets")]
    [SerializeField] private ObjectStateController noteHighlight;
    [SerializeField] private ObjectStateController fansHighlight; // optional: highlight all fans parent/object
    [SerializeField] private ObjectStateController ventHighlight;
    [SerializeField] private ObjectStateController exitHighlight;

    private enum Step { Note, Fans, Vent, Exit, Done }
    private Step currentStep;

    private int fansRepairedCount = 0;

    private void Start()
    {
        currentStep = Step.Note;

        if (objectiveUI)
            objectiveUI.SetInitial(noteText);

        SetActiveHighlight(noteHighlight);
    }

    // enables only one highlight at a time
    private void SetActiveHighlight(ObjectStateController target)
    {
        if (noteHighlight) noteHighlight.SetHighlightActive(false);
        if (fansHighlight) fansHighlight.SetHighlightActive(false);
        if (ventHighlight) ventHighlight.SetHighlightActive(false);
        if (exitHighlight) exitHighlight.SetHighlightActive(false);

        if (target) target.SetHighlightActive(true);
    }

    // Call when player picks the note
    public void OnNoteCollected()
    {
        if (currentStep != Step.Note) return;

        currentStep = Step.Fans;
        fansRepairedCount = 0;

        if (objectiveUI)
            objectiveUI.CompleteAndShowNext(fansText + $" (0/{fansToRepair})");

        SetActiveHighlight(fansHighlight);
    }

    // ✅ Call this from EACH fan after repair completes
    public void OnFanRepaired()
    {
        if (currentStep != Step.Fans) return;

        fansRepairedCount++;

        // update text while still repairing
        if (fansRepairedCount < fansToRepair)
        {
            if (objectiveUI)
                objectiveUI.SetInitial(fansText + $" ({fansRepairedCount}/{fansToRepair})");
            return;
        }

        // enough fans repaired → go to Vent step
        currentStep = Step.Vent;

        if (objectiveUI)
            objectiveUI.CompleteAndShowNext(ventText);

        SetActiveHighlight(ventHighlight);
    }

    // Call when player reaches ladder/vent
    public void OnVentFound()
    {
        if (currentStep != Step.Vent) return;

        currentStep = Step.Exit;

        if (objectiveUI)
            objectiveUI.CompleteAndShowNext(exitText);

        SetActiveHighlight(exitHighlight);
    }

    // Call when player uses main door
    public void OnExitUsed()
    {
        if (currentStep != Step.Exit) return;

        currentStep = Step.Done;

        if (objectiveUI)
            objectiveUI.CompleteAndShowNext("");

        SetActiveHighlight(null);
    }
}
