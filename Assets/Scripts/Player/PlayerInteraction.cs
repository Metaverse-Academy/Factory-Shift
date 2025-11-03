using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CanvasGroup promptCanvas;
    [SerializeField] private TMP_Text promptText;

    [Header("Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private bool showDebugRay = false;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float scalePop = 1.1f;

    private IInteractable currentTarget;
    private bool promptVisible;

    private void Awake()
    {
        if (promptCanvas)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.transform.localScale = Vector3.one;
        }
    }

    private void Update()
    {
        Vector3 origin = cameraTransform.position;
        Vector3 dir = cameraTransform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, interactDistance, interactableLayer))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                currentTarget = interactable;
                ShowPrompt(interactable.GetPrompt());
            }
            else HidePrompt();
        }
        else HidePrompt();

        if (showDebugRay)
            Debug.DrawRay(origin, dir * interactDistance, currentTarget != null ? Color.green : Color.red);
    }

    private void ShowPrompt(string text)
    {
        if (!promptCanvas) return;

        promptText.text = text;

        if (!promptVisible)
        {
            promptVisible = true;

            LeanTween.cancel(promptCanvas.gameObject);
            promptCanvas.transform.localScale = Vector3.one * 0.8f;

            LeanTween.scale(promptCanvas.gameObject, Vector3.one * scalePop, fadeDuration).setEaseOutBack();
            LeanTween.alphaCanvas(promptCanvas, 1f, fadeDuration);
        }
    }

    private void HidePrompt()
    {
        if (!promptCanvas || !promptVisible) return;

        promptVisible = false;

        LeanTween.cancel(promptCanvas.gameObject);
        LeanTween.scale(promptCanvas.gameObject, Vector3.one * 0.8f, fadeDuration).setEaseInBack();
        LeanTween.alphaCanvas(promptCanvas, 0f, fadeDuration);
    }

    // --- Input System ---
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentTarget != null)
        {
            currentTarget.Interact(gameObject);
        }
    }
}
