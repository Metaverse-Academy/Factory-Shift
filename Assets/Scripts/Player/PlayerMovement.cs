using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float sprintSpeed = 9f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDistanceCheck = 0.3f;
    [SerializeField] private float rayStartOffset = 0.06f;

    [Header("Crouch")]
    [SerializeField] private bool useToggleCrouch = true;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;

    [Header("Crawl / Prone")]
    [SerializeField] private bool useToggleCrawl = true;
    [SerializeField] private float crawlSpeed = 1.25f;
    [SerializeField] private float crawlHeight = 0.55f;

    [Header("Ceiling Check (for standing up)")]
    [Tooltip("Layers considered solid when checking if there is space to stand. Exclude the Player layer.")]
    [SerializeField] private LayerMask stanceBlockMask = ~0;

    [Header("Optional Camera Y per stance (local)")]
    [SerializeField] private float camY_Stand = 0.9f;
    [SerializeField] private float camY_Crouch = 0.6f;
    [SerializeField] private float camY_Crawl = 0.4f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [Tooltip("Speed > this means Run=true (only when not crouching/crawling).")]
    [SerializeField] private float runSpeedThreshold = 3.6f;  // tweak to your clips
    [Tooltip("Small deadzone to keep Idle from flickering when nearly still.")]
    //[SerializeField] private float idleDeadzone = 0.05f;
    //[Tooltip("If true, we’ll rotate the model toward planar movement direction (useful for 3rd-person).")]
    [SerializeField] private bool rotateModelToMove = false;
    [SerializeField] private Transform modelRoot; // optional, for model rotation only
    [SerializeField] private float idleDeadzone = 0.15f; // raise if needed
    [SerializeField] private float stopFriction = 20f;   // 20–35 feels good

    private Vector3 planarMoveDir;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private bool isCrawling;

    private float initialCapsuleRadius;
    private Vector3 initialCapsuleCenter;

    private enum Stance { Stand, Crouch, Crawl }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        initialCapsuleRadius = capsule.radius;
        initialCapsuleCenter = capsule.center;

        ApplyStance(Stance.Stand, force: true);
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void Update()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * rayStartOffset;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundDistanceCheck, groundLayer, QueryTriggerInteraction.Ignore);

        UpdateAnimator();
    }

    private void HandleMovement()
    {
        Vector3 f = cameraTransform.forward; f.y = 0f; f.Normalize();
        Vector3 r = cameraTransform.right;  r.y = 0f; r.Normalize();

        Vector3 desiredPlanar = f * moveInput.y + r * moveInput.x;
        planarMoveDir = desiredPlanar.sqrMagnitude > 1e-4f ? desiredPlanar.normalized : Vector3.zero;

        float targetSpeed = isCrawling ? crawlSpeed :
                           (isCrouching ? crouchSpeed :
                           (isSprinting ? sprintSpeed : walkSpeed));

        Vector3 targetVelH = planarMoveDir * targetSpeed;
        

     Vector3 v = rb.linearVelocity; // use rb.velocity if standard Rigidbody
Vector3 vH = Vector3.Lerp(new Vector3(v.x, 0f, v.z), targetVelH, acceleration * Time.fixedDeltaTime);

if (planarMoveDir == Vector3.zero)
{
    // aggressively stop horizontal sliding when no input
    vH = Vector3.MoveTowards(new Vector3(v.x, 0f, v.z), Vector3.zero, stopFriction * Time.fixedDeltaTime);
}

rb.linearVelocity = new Vector3(vH.x, v.y, vH.z); // or rb.velocity

    }

    // ---- Stance management ----
    private void ApplyStance(Stance stance, bool force = false)
    {
        float targetHeight = standingHeight;
        float camY = camY_Stand;

        switch (stance)
        {
            case Stance.Stand:
                targetHeight = standingHeight;
                camY = camY_Stand;
                break;
            case Stance.Crouch:
                targetHeight = crouchHeight;
                camY = camY_Crouch;
                break;
            case Stance.Crawl:
                targetHeight = crawlHeight;
                camY = camY_Crawl;
                break;
        }

        if (!force)
        {
            float currentHeight = capsule.height;
            bool gettingTaller = targetHeight > currentHeight + 0.001f;
            if (gettingTaller && !HasSpaceFor(targetHeight))
                return;
        }

        // Flags
        isCrawling = (stance == Stance.Crawl);
        isCrouching = (stance == Stance.Crouch);
        if (isCrouching || isCrawling) isSprinting = false; // can’t sprint while low

        // Collider resize keeping feet anchored
        capsule.height = targetHeight;
        capsule.center = new Vector3(initialCapsuleCenter.x, targetHeight * 0.5f, initialCapsuleCenter.z);

        // Camera offset
        if (cameraTransform != null)
        {
            Vector3 lp = cameraTransform.localPosition;
            cameraTransform.localPosition = new Vector3(lp.x, camY, lp.z);
        }

        // Animator stance flags immediately
        if (animator)
        {
            animator.SetBool("Crouch", isCrouching);
            animator.SetBool("Crawl",  isCrawling);
        }
    }

    private bool HasSpaceFor(float targetHeight)
    {
        float radius = Mathf.Max(0.05f, capsule.radius * 0.98f);

        Vector3 worldCenter = transform.TransformPoint(capsule.center);
        float half = capsule.height * 0.5f;
        float feetY = worldCenter.y - half + radius;

        Vector3 feet = new Vector3(worldCenter.x, feetY, worldCenter.z);
        Vector3 head = new Vector3(worldCenter.x, feetY + (targetHeight - radius), worldCenter.z);

        int selfLayer = gameObject.layer;
        int maskNoSelf = stanceBlockMask & ~(1 << selfLayer);

        bool blocked = Physics.CheckCapsule(feet, head, radius, maskNoSelf, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    private void UpdateAnimator()
    {
        if (!animator) return;

      Vector3 vel = rb.linearVelocity; // or rb.velocity
      float horizSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
        float speedParam = (horizSpeed <= idleDeadzone) ? 0f : horizSpeed;
      animator.SetFloat("Speed", speedParam);

        // Core parameters
        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("Crouch",   isCrouching);
        animator.SetBool("Crawl",    isCrawling);

        // Speed param drives Idle/Walk/Run blend tree (0..max)
        //float speedParam = (horizSpeed <= idleDeadzone) ? 0f : horizSpeed;
        //animator.SetFloat("Speed", speedParam);

        // Run flag only when upright & actually moving fast
        bool run = !isCrouching && !isCrawling && horizSpeed > runSpeedThreshold && isSprinting;
        animator.SetBool("Run", run);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundDistanceCheck);
    }

    #region Input System Callbacks

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // No jump while low (change if you want crouch-jumps)
        if (isCrouching || isCrawling) return;

        if (isGrounded)
        {
            Vector3 cur = rb.linearVelocity; // If using Rigidbody, change to rb.velocity
            if (cur.y < 0f) cur.y = 0f;
            rb.linearVelocity = cur;         // If using Rigidbody, change to rb.velocity

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Trigger jump animation
            if (animator) animator.SetTrigger("Jump");
        }
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (!isCrouching && !isCrawling) isSprinting = true;
        }
        else if (ctx.canceled) isSprinting = false;
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (useToggleCrouch)
        {
            if (ctx.performed)
            {
                if (isCrawling)
                {
                    ApplyStance(Stance.Crouch);
                }
                else if (isCrouching)
                {
                    ApplyStance(Stance.Stand);
                }
                else
                {
                    ApplyStance(Stance.Crouch);
                }
            }
        }
        else
        {
            if (ctx.performed)
            {
                if (!isCrawling) ApplyStance(Stance.Crouch);
            }
            else if (ctx.canceled)
            {
                ApplyStance(Stance.Stand);
            }
        }
    }

    public void OnCrawl(InputAction.CallbackContext ctx)
    {
        if (!useToggleCrawl)
        {
            // hold-to-crawl
            if (ctx.performed)
            {
                ApplyStance(Stance.Crawl);
            }
            else if (ctx.canceled)
            {
                if (HasSpaceFor(standingHeight))      ApplyStance(Stance.Stand);
                else if (HasSpaceFor(crouchHeight))   ApplyStance(Stance.Crouch);
            }
            return;
        }

        // toggle behavior
        if (ctx.performed)
        {
            if (isCrawling)
            {
                if (HasSpaceFor(standingHeight))      ApplyStance(Stance.Stand);
                else if (HasSpaceFor(crouchHeight))   ApplyStance(Stance.Crouch);
            }
            else
            {
                ApplyStance(Stance.Crawl);
            }
        }
    }

    #endregion
}
