using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ObjectiveUI objectiveUI;

    [TextArea] public string noteText      = "Read the note";
    [TextArea] public string ventText      = "Look for the vent";
    [TextArea] public string exitText      = "Exit the building";

    [Header("Highlight Targets")]
    [SerializeField] private ObjectStateController noteHighlight;
    [SerializeField] private ObjectStateController ventHighlight;
    [SerializeField] private ObjectStateController exitHighlight;

    private enum Step { Note, Vent, Exit, Done }
    private Step currentStep;

    private void Start()
    {
        currentStep = Step.Note;

        // اول هدف
        if (objectiveUI) 
            objectiveUI.SetInitial(noteText);

        SetActiveHighlight(noteHighlight);
    }

    // يجعل فقط تارجت واحد مفعّل
    private void SetActiveHighlight(ObjectStateController target)
    {
        if (noteHighlight) noteHighlight.SetHighlightActive(false);
        if (ventHighlight) ventHighlight.SetHighlightActive(false);
        if (exitHighlight) exitHighlight.SetHighlightActive(false);

        if (target) target.SetHighlightActive(true);
    }

    // ↙️ استدعها عندما اللاعب يلتقط المذكرة
    public void OnNoteCollected()
    {
        if (currentStep != Step.Note) return;

        currentStep = Step.Vent;

        if (objectiveUI)
            objectiveUI.CompleteAndShowNext(ventText);

        SetActiveHighlight(ventHighlight);
    }

    // ↙️ استدعها عندما يصل للسلّم / الفتحة
    public void OnVentFound()
    {
        if (currentStep != Step.Vent) return;

        currentStep = Step.Exit;

        if (objectiveUI)
            objectiveUI.CompleteAndShowNext(exitText);

        SetActiveHighlight(exitHighlight);
    }

    // ↙️ استدعها عندما يخرج من الباب الرئيسي
    public void OnExitUsed()
    {
        if (currentStep != Step.Exit) return;

        currentStep = Step.Done;

        if (objectiveUI)
            objectiveUI.CompleteAndShowNext(""); // أو تخليها فاضية/تعطّل الـ UI

        SetActiveHighlight(null); // لا هايلايت بعد الآن
    }
}
