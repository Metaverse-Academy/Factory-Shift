using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class VentPortalInteract : MonoBehaviour
{
    [Header("Who can use")]
    [SerializeField] private string playerTag = "Player";

    [Header("Destination")]
    [SerializeField] private Transform destination;
    [Tooltip("Yaw taken from this transform if set, else from Destination.")]
    [SerializeField] private Transform faceDirection;

    [Header("UI & Input")]
    [Tooltip("Input Action (Button) bound to E (or your interact key)")]
    [SerializeField] private InputActionReference interactAction;   // E
    [Tooltip("Shown when player is inside the trigger and allowed to interact")]
    [SerializeField] private GameObject promptUI;

    [Header("Behavior")]
    [Tooltip("If true, call ForceEnterCrawl() after teleport (use for entering vents).")]
    [SerializeField] private bool forceCrawlAfterTeleport = true;
    [Tooltip("Prevents an immediate re-trigger bounce at the destination.")]
    [SerializeField] private float reenterLockout = 0.25f;

    // per-player lockout by instance id
    private static readonly Dictionary<int, float> lockoutUntil = new();

    // cached per-entrant
    private bool inRange;
    private Transform playerRoot;
    private PlayerMovement playerMove;
    private Rigidbody playerRb;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }
        if (promptUI) promptUI.SetActive(false);
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
        // accept child colliders, check root tag
        var root = other.transform.root;
        if (!root.CompareTag(playerTag)) return;

        // lockout: don’t immediately re-trigger the arrival portal
        int id = root.GetInstanceID();
        if (lockoutUntil.TryGetValue(id, out float until) && Time.time < until) return;

        // cache refs
        playerRoot = root;
        playerMove = root.GetComponent<PlayerMovement>();
        playerRb   = root.GetComponent<Rigidbody>();
        inRange = true;

        if (promptUI) promptUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        var root = other.transform.root;
        if (!root.CompareTag(playerTag)) return;

        inRange = false;
        playerRoot = null;
        playerMove = null;
        playerRb = null;

        if (promptUI) promptUI.SetActive(false);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!inRange) return;                  // must be inside trigger
        if (!playerRoot || !destination) return;

        // Teleport now
        TeleportPlayer();

        // Lock out immediate re-trigger at the landing portal
        lockoutUntil[playerRoot.GetInstanceID()] = Time.time + reenterLockout;

        // Hide prompt
        if (promptUI) promptUI.SetActive(false);

        // Clear state
        inRange = false;
        playerRoot = null; playerMove = null; playerRb = null;
    }

    private void TeleportPlayer()
    {

        // zero momentum to avoid sliding
        if (playerRb)
        {
#if UNITY_6000_0_OR_NEWER
            playerRb.linearVelocity = Vector3.zero;
#else
            playerRb.velocity = Vector3.zero;
#endif
            playerRb.angularVelocity = Vector3.zero;
        }

        
        // yaw-only rotation from faceDirection or destination
        float yaw = (faceDirection ? faceDirection.rotation : destination.rotation).eulerAngles.y;
        Quaternion yawOnly = Quaternion.Euler(0f, yaw, 0f);

        playerRoot.SetPositionAndRotation(destination.position, yawOnly);
        
        Physics.SyncTransforms();

   // optional: force crawl only for ENTER portals
if (forceCrawlAfterTeleport)
{
    playerMove?.ForceEnterCrawl();
}
else
{
    // EXIT portals: immediately try to go upright
    playerMove?.ExitVentUpright(false); // pass true if you want to force stand regardless of headroom
}

// snap animator on new pose
var anim = playerRoot.GetComponentInChildren<Animator>();
if (anim) anim.Update(0f);

    }

    private void OnDrawGizmos()
    {
        if (!destination) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(destination.position, 0.25f);
        Gizmos.DrawRay(destination.position, destination.forward * 0.6f);
    }
}
