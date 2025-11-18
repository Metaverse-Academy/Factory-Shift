using UnityEngine;
using UnityEngine.InputSystem;

public class NotebookInteraction : MonoBehaviour
{
    public Canvas notebookCanvas;
    public GameObject player;

    bool isOpen = false;
    bool playerNear = false;

    PlayerInput playerInput;
    InputAction interactAction;

    void Start()
    {
        playerInput = player.GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
    }

    void Update()
    {
        if (playerNear && !isOpen && interactAction.WasPerformedThisFrame())
        {
            OpenNotebook();
        }

        if (isOpen && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame && !Mouse.current.rightButton.wasPressedThisFrame)
                CloseNotebook();
        }
    }

    void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        playerNear = true;
        Debug.Log("PLAYER ENTERED NOTEBOOK AREA");
    }
}

void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player"))
    {
        playerNear = false;
        Debug.Log("PLAYER LEFT NOTEBOOK AREA");
    }
}


    void OpenNotebook()
    {
        isOpen = true;
        notebookCanvas.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerInput.DeactivateInput();
    }

    void CloseNotebook()
    {
        isOpen = false;
        notebookCanvas.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerInput.ActivateInput();
    }
}
