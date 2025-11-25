using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class NoteReader : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Things to disable while reading (PlayerMovement, CameraRotate, etc).")]
    [SerializeField] private MonoBehaviour[] disableWhileReading;

    [Header("Input & UI")]
    [SerializeField] private InputActionReference interactAction; // bind to E
    [SerializeField] private GameObject promptUI;                 // "E to open note"
    [SerializeField] private GameObject noteUI;                   // panel with book/pages

    // [Header("Objectives (optional)")]
    // [SerializeField] private ObjectiveManager objectiveManager;
    [Tooltip("Call OnNoteCollected once when player finishes reading (closes the note).")]
    [SerializeField] private bool reportNoteObjectiveOnClose = true;
    [Header("Objectives (optional)")]
[SerializeField] private Night1ObjectiveManager night1Objectives;
private bool reportedNote = false;


    private bool inRange;
    private bool noteOpen;
    private bool noteObjectiveReported = false;

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

        if (promptUI) promptUI.SetActive(false);
        if (noteUI)   noteUI.SetActive(false);
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inRange = true;

        if (!noteOpen && promptUI)
            promptUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inRange = false;

        if (promptUI) promptUI.SetActive(false);

        // if player walks away while reading, close the note
        if (noteOpen)
            CloseNote();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!inRange) return;   // only if close to note

        if (!noteOpen)
        {
            OpenNote();
        }
        else
        {
            CloseNote();
        }
    }

   private void OpenNote()
{
    noteOpen = true;

    if (promptUI) promptUI.SetActive(false);
    if (noteUI) noteUI.SetActive(true);

    foreach (var comp in disableWhileReading)
        if (comp) comp.enabled = false;

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    // ✅ report objective once
    if (!reportedNote)
    {
        reportedNote = true;
        night1Objectives?.OnNoteRead();
    }
}


    private void CloseNote()
    {
        noteOpen = false;

        if (noteUI) noteUI.SetActive(false);

        // re-enable movement / looking
        foreach (var comp in disableWhileReading)
        {
            if (comp) comp.enabled = true;
        }

        // relock mouse for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // show prompt again if still inside trigger
        if (inRange && promptUI)
            promptUI.SetActive(true);

        // 🔹 Report objective ONCE when the note was properly read & closed
        if (reportNoteObjectiveOnClose && !noteObjectiveReported)
        {
            noteObjectiveReported = true;
            night1Objectives?.OnNoteRead();
        }
    }
}
