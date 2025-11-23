using UnityEngine;

public class MultiRepairObjective : MonoBehaviour
{
    [Header("Fans to repair")]
    [SerializeField] private RepairableFixable[] fans;

    [Header("Objective UI")]
    [SerializeField] private ObjectiveUI objectiveUI;
    [SerializeField] private string objectiveText = "Repair both fans";
    [SerializeField] private string nextObjectiveText = "Go Home";

    [Header("Highlights")]
    [Tooltip("Highlight these while repairing fans (you can drag each fan highlight here).")]
    [SerializeField] private ObjectStateController[] fanHighlights;

    [Tooltip("Highlight this after all fans are repaired (door / exit / vent etc).")]
    [SerializeField] private ObjectStateController nextHighlight;

    private int repairedCount;

    private void Start()
    {
        repairedCount = 0;

        // initial objective text
        if (objectiveUI != null)
            objectiveUI.SetInitial(objectiveText);

        // ✅ turn ON fan highlights, turn OFF next highlight
        SetFanHighlights(true);
        SetNextHighlight(false);

        // subscribe to all fans
        foreach (var fan in fans)
        {
            if (fan == null) continue;

            // if already repaired (just in case)
            if (fan.IsRepaired) repairedCount++;

            fan.OnRepaired += HandleFanRepaired;
        }

        CheckCompletion();
    }

    private void OnDestroy()
    {
        // unsubscribe safety
        foreach (var fan in fans)
        {
            if (fan == null) continue;
            fan.OnRepaired -= HandleFanRepaired;
        }
    }

    private void HandleFanRepaired(RepairableFixable fan)
    {
        repairedCount++;
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (repairedCount >= fans.Length)
        {
            // ✅ Objective complete only when ALL fans repaired
            if (objectiveUI != null)
                objectiveUI.CompleteAndShowNext(nextObjectiveText);

            Debug.Log("All fans repaired! Objective done.");

            // ✅ switch highlights
            SetFanHighlights(false);
            SetNextHighlight(true);
        }
        else
        {
            Debug.Log($"Fans repaired: {repairedCount}/{fans.Length}");

            // optional: update UI with count
            if (objectiveUI != null)
                objectiveUI.SetInitial($"{objectiveText} ({repairedCount}/{fans.Length})");
        }
    }

    // ---------------- Highlight helpers ----------------
    private void SetFanHighlights(bool on)
    {
        if (fanHighlights == null) return;
        foreach (var h in fanHighlights)
        {
            if (h != null) h.SetHighlightActive(on);
        }
    }

    private void SetNextHighlight(bool on)
    {
        if (nextHighlight != null)
            nextHighlight.SetHighlightActive(on);
    }
}
