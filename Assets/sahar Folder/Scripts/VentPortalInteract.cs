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
    [SerializeField] private InputActionReference interactAction;   // E
    [SerializeField] private GameObject promptUI;

    [Header("Behavior")]
    [SerializeField] private bool forceCrawlAfterTeleport = true;
    [SerializeField] private float reenterLockout = 0.25f;

    [Header("Objective Hook (optional)")]
    [SerializeField] private MultiRepairObjective multiObjective;
    [SerializeField] private Night1ObjectiveManager night1ObjectiveManager;
   

    public enum PortalMode { None, EnterVents, ExitVents }
    [SerializeField] private PortalMode portalMode = PortalMode.None;

    private static readonly Dictionary<int, float> lockoutUntil = new();

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
        var root = other.transform.root;
        if (!root.CompareTag(playerTag)) return;

        int id = root.GetInstanceID();
        if (lockoutUntil.TryGetValue(id, out float until) && Time.time < until) return;

        playerRoot = root;
        playerMove = root.GetComponent<PlayerMovement>();
        playerRb = root.GetComponent<Rigidbody>();
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
        if (!inRange) return;
        if (!playerRoot || !destination) return;

        TeleportPlayer();

        lockoutUntil[playerRoot.GetInstanceID()] = Time.time + reenterLockout;

        // ✅ Notify objective system
        if (multiObjective != null)
        {
            if (portalMode == PortalMode.EnterVents)
                multiObjective.OnEnterVents();
            else if (portalMode == PortalMode.ExitVents)
                multiObjective.OnExitVents();
        }
           if (night1ObjectiveManager != null)
        {
            if (portalMode == PortalMode.EnterVents)
                night1ObjectiveManager.OnEnterVents();
            else if (portalMode == PortalMode.ExitVents)
                night1ObjectiveManager.OnExitVents();
        }
    

        if (promptUI) promptUI.SetActive(false);

        inRange = false;
        playerRoot = null;
        playerMove = null;
        playerRb = null;
    }

    private void TeleportPlayer()
    {
        if (playerRb)
        {
#if UNITY_6000_0_OR_NEWER
            playerRb.linearVelocity = Vector3.zero;
#else
            playerRb.velocity = Vector3.zero;
#endif
            playerRb.angularVelocity = Vector3.zero;
        }

        float yaw = (faceDirection ? faceDirection.rotation : destination.rotation).eulerAngles.y;
        Quaternion yawOnly = Quaternion.Euler(0f, yaw, 0f);

        playerRoot.SetPositionAndRotation(destination.position, yawOnly);
        Physics.SyncTransforms();

        if (forceCrawlAfterTeleport)
            playerMove?.ForceEnterCrawl();
        else
            playerMove?.ExitVentUpright(false);

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
