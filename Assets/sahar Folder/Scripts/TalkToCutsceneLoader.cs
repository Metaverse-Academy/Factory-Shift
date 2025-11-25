using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class CutsceneRoute
{
    public static string ReturnSceneName;   // where to go back
}

[RequireComponent(typeof(Collider))]
public class TalkToCutsceneLoader : MonoBehaviour
{
    [Header("Interact")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private InputActionReference interactAction; // bind to E
    [SerializeField] private GameObject promptUI;                 // "E TO TALK" object

    [Header("Cutscene")]
    [Tooltip("Exact scene name of your cutscene scene (must be in Build Settings).")]
    [SerializeField] private string cutsceneSceneName = "Cutscene_Scene";

    private bool playerInRange;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        if (interactAction) interactAction.action.performed += OnInteract;
        if (promptUI) promptUI.SetActive(false);
    }

    private void OnDisable()
    {
        if (interactAction) interactAction.action.performed -= OnInteract;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        if (promptUI) promptUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        if (promptUI) promptUI.SetActive(false);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;

        // remember which scene to return to
        CutsceneRoute.ReturnSceneName = SceneManager.GetActiveScene().name;

        if (promptUI) promptUI.SetActive(false);

        // load the cutscene scene (single-mode)
        SceneManager.LoadScene(cutsceneSceneName, LoadSceneMode.Single);
    }
}
