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
    [SerializeField] private InputActionReference interactAction;   // زر E
    [SerializeField] private InputActionReference gamepadReadAction; // زر R1
    [SerializeField] private GameObject promptUI;

    [Header("Different Notes")]
    [SerializeField] private GameObject keyboardNoteUI;  // يظهر عند ضغط E
    [SerializeField] private GameObject gamepadNoteUI;   // يظهر عند ضغط R1

    [Header("Prompt Variants")]
    [SerializeField] private GameObject keyboardPrompt;
    [SerializeField] private GameObject gamepadPrompt;

    [Header("Objectives (optional)")]
    [SerializeField] private Night1ObjectiveManager night1Objectives;
    [SerializeField] private bool reportNoteObjectiveOnClose = true;

    private bool inRange;
    private bool noteOpen;
    private bool hasReportedNoteObjective = false;

    private GameObject currentOpenedNote = null;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        // Keyboard (E)
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }

        // Gamepad (R1)
        if (gamepadReadAction != null)
        {
            gamepadReadAction.action.Enable();
            gamepadReadAction.action.performed += OnGamepadRead;
        }

        if (promptUI) promptUI.SetActive(false);
        if (keyboardNoteUI) keyboardNoteUI.SetActive(false);
        if (gamepadNoteUI) gamepadNoteUI.SetActive(false);

        if (InputSchemeUIManager.Instance != null)
            InputSchemeUIManager.Instance.OnSchemeChanged += HandleSchemeChanged;
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }

        if (gamepadReadAction != null)
        {
            gamepadReadAction.action.performed -= OnGamepadRead;
            gamepadReadAction.action.Disable();
        }

        if (InputSchemeUIManager.Instance != null)
            InputSchemeUIManager.Instance.OnSchemeChanged -= HandleSchemeChanged;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inRange = true;

        if (!noteOpen)
            RefreshPromptVisual();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inRange = false;

        if (promptUI) promptUI.SetActive(false);

        if (noteOpen)
            CloseNote();
    }

    // =============== INPUT HANDLERS =================

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!inRange) return;

        if (!noteOpen)
            OpenNote(keyboardNoteUI);  // فتح كانفس الكيبورد
        else
            CloseNote();
    }

    private void OnGamepadRead(InputAction.CallbackContext ctx)
    {
        if (!inRange) return;

        if (!noteOpen)
            OpenNote(gamepadNoteUI);  // فتح كانفس الكنترولر
        else
            CloseNote();

            Debug.Log("Gamepad Read Action Triggered");
    }

    // =============== OPEN & CLOSE NOTE ===============

    private void OpenNote(GameObject ui)
    {
        if (ui == null) return;

        noteOpen = true;
        currentOpenedNote = ui;

        if (promptUI) promptUI.SetActive(false);

        ui.SetActive(true);

        foreach (var comp in disableWhileReading)
            if (comp) comp.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseNote()
    {
        noteOpen = false;

        if (currentOpenedNote)
            currentOpenedNote.SetActive(false);

        currentOpenedNote = null;

        foreach (var comp in disableWhileReading)
            if (comp) comp.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (inRange)
            RefreshPromptVisual();
        else if (promptUI)
            promptUI.SetActive(false);

        if (reportNoteObjectiveOnClose && !hasReportedNoteObjective)
        {
            hasReportedNoteObjective = true;
            night1Objectives?.OnNoteRead();
        }
    }

    // =============== PROMPT DISPLAY ===============

    private void RefreshPromptVisual()
    {
        if (!promptUI) return;

        bool shouldShow = inRange && !noteOpen;

        if (!shouldShow)
        {
            promptUI.SetActive(false);
            if (keyboardPrompt) keyboardPrompt.SetActive(false);
            if (gamepadPrompt)  gamepadPrompt.SetActive(false);
            return;
        }

        promptUI.SetActive(true);

        bool useGamepad =
            InputSchemeUIManager.Instance != null &&
            InputSchemeUIManager.Instance.IsGamepad;

        if (keyboardPrompt) keyboardPrompt.SetActive(!useGamepad);
        if (gamepadPrompt) gamepadPrompt.SetActive(useGamepad);
    }

    private void HandleSchemeChanged(bool isGamepad)
    {
        if (inRange && !noteOpen)
            RefreshPromptVisual();
    }
}
