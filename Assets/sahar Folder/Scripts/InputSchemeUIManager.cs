using UnityEngine;
using UnityEngine.InputSystem;

public class InputSchemeUIManager : MonoBehaviour
{
    public static InputSchemeUIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Control Scheme Names (must match your Input Actions asset)")]
    [SerializeField] private string keyboardMouseScheme = "Keyboard&Mouse";
    [SerializeField] private string gamepadScheme       = "Gamepad";

    public bool IsGamepad { get; private set; }

    public System.Action<bool> OnSchemeChanged; // bool = isGamepad

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!playerInput)
            playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        UpdateSchemeFromPlayerInput();
    }

    // Hook this in PlayerInput's "On Controls Changed" event in the Inspector
    public void OnControlsChanged(PlayerInput pi)
    {
        UpdateSchemeFromPlayerInput();
    }

    private void UpdateSchemeFromPlayerInput()
    {
        if (!playerInput) return;

        bool newIsGamepad = playerInput.currentControlScheme == gamepadScheme;

        if (newIsGamepad == IsGamepad)
            return; // no change

        IsGamepad = newIsGamepad;
        // notify listeners (UI)
        OnSchemeChanged?.Invoke(IsGamepad);
        Debug.Log("Scheme changed. Gamepad = " + IsGamepad);
    }
}
