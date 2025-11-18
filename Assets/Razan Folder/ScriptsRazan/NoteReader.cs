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

    private bool inRange;
    private bool noteOpen;

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
        if (noteUI)   noteUI.SetActive(false);
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
        // If not in range, ignore
        if (!inRange) return;

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
        if (noteUI)   noteUI.SetActive(true);

        // disable movement / looking scripts
        foreach (var comp in disableWhileReading)
        {
            if (comp) comp.enabled = false;
        }

        // unlock mouse so player can click / flip pages
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // if your "book script" needs a reset, you can call it here:
        // noteUI.GetComponent<YourBookScript>()?.Open();
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
    }
}
