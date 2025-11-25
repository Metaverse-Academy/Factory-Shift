using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class LadderVentInteract : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform playerCamera;

    [Header("Vent Destination")]
    [SerializeField] private Transform ventSpawn;
    [SerializeField] private Transform ventForwardRef;

    [Header("Input & UI")]
    [SerializeField] private InputActionReference interactAction; // E
    [SerializeField] private GameObject promptUI;

    [Header("Look Check")]
    [SerializeField, Range(5f, 60f)] private float viewAngle = 20f;
    [SerializeField] private float maxViewDistance = 25f;
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask losMask = ~0;

    private bool inRange;
    private Transform currentPlayer;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();                 // <-- IMPORTANT
            interactAction.action.performed += OnInteract;  // subscribe
        }
        if (promptUI) promptUI.SetActive(false);
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;  // unsubscribe
            interactAction.action.Disable();                // <-- IMPORTANT
        }
    }

    private void Update()
    {
        // Keep prompt in sync while in the trigger
        if (inRange && promptUI)
            promptUI.SetActive(IsLookingAtVent());
    }

    private void OnTriggerEnter(Collider other)
    {
        // accept either the collider or its root tagged "Player"
        if (!(other.CompareTag(playerTag) || other.transform.root.CompareTag(playerTag))) return;

        inRange = true;                                     // <-- use the SAME flag the interact checks
        currentPlayer = other.transform.root;
        TryFindPlayerRefs(currentPlayer);

        if (promptUI) promptUI.SetActive(IsLookingAtVent());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!(other.CompareTag(playerTag) || other.transform.root.CompareTag(playerTag))) return;

        inRange = false;
        currentPlayer = null;
        if (promptUI) promptUI.SetActive(false);
    }

    private bool IsLookingAtVent()
    {
        if (!playerCamera || !ventSpawn) return false;

        Vector3 toVent = ventSpawn.position - playerCamera.position;
        float dist = toVent.magnitude;
        if (dist > maxViewDistance) return false;

        float ang = Vector3.Angle(playerCamera.forward, toVent);
        if (ang > viewAngle) return false;

        if (requireLineOfSight)
        {
            if (Physics.Raycast(playerCamera.position, toVent.normalized, out RaycastHit hit,
                                dist + 0.1f, losMask, QueryTriggerInteraction.Ignore))
            {
                // allow if what we hit is basically the vent point or its parent
                if (hit.transform != ventSpawn && hit.transform != ventSpawn.parent) return false;
            }
        }
        return true;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!inRange) return;                 // you must be in the trigger
        if (!IsLookingAtVent()) return;       // and actually looking at the vent

        TeleportToVent();
    }

    private void TeleportToVent()
    {
        if (!playerRoot || !ventSpawn || !playerMovement || !playerRb) return;

        // zero momentum
        playerRb.linearVelocity = Vector3.zero;     // use .velocity if your RB doesn't have linearVelocity
        playerRb.angularVelocity = Vector3.zero;

        // move & face the vent
        playerRoot.position = ventSpawn.position;
        Vector3 fwd = ventForwardRef ? ventForwardRef.forward : ventSpawn.forward;
        fwd.y = 0f; if (fwd.sqrMagnitude < 1e-6f) fwd = playerRoot.forward;
        playerRoot.rotation = Quaternion.LookRotation(fwd, Vector3.up);

        // enter crawl immediately (force headroom)
        playerMovement.ForceEnterCrawl();

        if (promptUI) promptUI.SetActive(false);
    }

    private void TryFindPlayerRefs(Transform playerRootGuess)
    {
        if (!playerRoot)      playerRoot = playerRootGuess;
        if (!playerMovement)  playerMovement = playerRootGuess.GetComponentInChildren<PlayerMovement>();
        if (!playerRb)        playerRb = playerRootGuess.GetComponentInChildren<Rigidbody>();
        if (!playerCamera)
        {
            var cam = playerRootGuess.GetComponentInChildren<Camera>();
            if (cam) playerCamera = cam.transform;
        }
    }
}
