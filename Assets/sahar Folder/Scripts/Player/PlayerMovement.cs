using Unity.Cinemachine;
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
    //[SerializeField] private float crawlHeight = 0.55f;

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
    [SerializeField] private float idleDeadzone = 0.05f;
    [Tooltip("If true, we’ll rotate the model toward planar movement direction (useful for 3rd-person).")]
    [SerializeField] private bool rotateModelToMove = false;
    [SerializeField] private Transform modelRoot;
    [Header("Footsteps")]
    [Tooltip("Randomly picked for each step.")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.5f;
    [Tooltip("Audio source position for footsteps. If null, uses this transform.")]
    [SerializeField] private Transform footstepOrigin;
    [Tooltip("Base steps per second when moving at walkSpeed while standing.")]
    [SerializeField] private float baseStepsPerSecond = 1.8f;
    [Tooltip("Stride multiplier per stance (affects cadence).")]
    [SerializeField] private float runStrideMult = 1.75f;
    [SerializeField] private float crouchStrideMult = 0.7f;
    [Tooltip("Minimum horizontal speed to start footsteps.")]
    [SerializeField] private float footstepSpeedThreshold = 0.12f;

    [Header("Landing SFX (optional)")]
    [SerializeField] private AudioClip landingClip;
    [SerializeField, Range(0f, 1f)] private float landingVolume = 0.6f;


    private Vector3 planarMoveDir;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private bool isCrawling;
    private float stepAccumulator;
    private bool wasGrounded;
    public bool IsCrawling => isCrawling;
    public bool IsCrouching => isCrouching;
    private bool inVent;       
    public bool InVent => inVent; 


    private float initialCapsuleRadius;
    private Vector3 initialCapsuleCenter;
    
  [Header("Colliders")]
[SerializeField] private CapsuleCollider standCollider; // vertical
[SerializeField] private CapsuleCollider crawlCollider; // horizontal child

    public float LookaheadSettings { get; private set; }
    public Vector3 Velocity { get; private set; }

    private enum Stance { Stand, Crouch, Crawl }

private void Awake()
{
    rb = GetComponent<Rigidbody>();

    // if not assigned in inspector, assume the one on this GameObject is the standing collider
    if (!standCollider) standCollider = GetComponent<CapsuleCollider>();
    capsule = standCollider;                  // use vertical as our main capsule reference

    if (crawlCollider)
        crawlCollider.enabled = false;        // disable prone collider at start

    rb.interpolation = RigidbodyInterpolation.Interpolate;
    rb.constraints = RigidbodyConstraints.FreezeRotationX |
                     RigidbodyConstraints.FreezeRotationZ |
                     RigidbodyConstraints.FreezeRotationY;

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
        HandleFootsteps();
        HandleLandingSfx();

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

        Vector3 v = rb.linearVelocity; // If using Rigidbody, change to rb.velocity
        Vector3 vH = Vector3.Lerp(new Vector3(v.x, 0f, v.z), targetVelH, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(vH.x, v.y, vH.z); // If using Rigidbody, change to rb.velocity

        // Optional face movement direction (3rd-person)
        if (rotateModelToMove && modelRoot != null && planarMoveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(planarMoveDir, Vector3.up);
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, look, 12f * Time.deltaTime);
        }
    }

    // ---- Stance management ----
    private void ApplyStance(Stance stance, bool force = false)
{
    float targetHeight = standingHeight;
    float camY = camY_Stand;

    // ------------ choose target height & camera Y ------------
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
            // we won’t use the vertical capsule for crawl,
            // only change camera height here
            camY = camY_Crawl;
            break;
    }

    // ------------ space check only when getting taller AND using vertical collider ------------
    bool usingVertical = (stance == Stance.Stand || stance == Stance.Crouch);
    if (usingVertical && !force)
    {
        float currentHeight = capsule.height;
        bool gettingTaller = targetHeight > currentHeight + 0.001f;
        if (gettingTaller && !HasSpaceFor(targetHeight))
            return;
    }

    // ------------ flags ------------
    isCrawling  = (stance == Stance.Crawl);
    isCrouching = (stance == Stance.Crouch);
    if (isCrouching || isCrawling) isSprinting = false;

    // ------------ collider switching ------------
    if (usingVertical)
    {
        // Stand / Crouch → vertical collider ON, crawl collider OFF
        if (standCollider) standCollider.enabled = true;
        if (crawlCollider) crawlCollider.enabled = false;

        // resize vertical capsule (feet anchored using your old logic / center)
        capsule.height = targetHeight;
        capsule.center = new Vector3(
            initialCapsuleCenter.x,
            targetHeight * 0.5f,
            initialCapsuleCenter.z
        );
    }
    else
    {
        // Crawl → vertical collider OFF, prone collider ON
        if (standCollider) standCollider.enabled = false;
        if (crawlCollider) crawlCollider.enabled = true;
    }

    // ------------ camera offset ------------
    if (cameraTransform != null)
    {
        Vector3 lp = cameraTransform.localPosition;
        cameraTransform.localPosition = new Vector3(lp.x, camY, lp.z);
    }

    // ------------ animator flags ------------
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
    public void Teleport(Vector3 position, Quaternion rotation)
{
    rb.linearVelocity = Vector3.zero;      // or rb.linearVelocity if that’s what you use
    rb.angularVelocity = Vector3.zero;

    // yaw only
    var yawOnly = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
    transform.SetPositionAndRotation(position, yawOnly);

    Physics.SyncTransforms();
    if (animator) animator.Update(0f);

    // (remove those LookaheadSettings lines; they’re not used for rotation)
}


    private void UpdateAnimator()
    {
        if (!animator) return;

        // Horizontal speed magnitude
        Vector3 v = rb.linearVelocity; // change to rb.velocity if needed
        float horizSpeed = new Vector3(v.x, 0f, v.z).magnitude;

        // Core parameters
        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("Crouch",   isCrouching);
        animator.SetBool("Crawl",    isCrawling);

        // Speed param drives Idle/Walk/Run blend tree (0..max)
        float speedParam = (horizSpeed <= idleDeadzone) ? 0f : horizSpeed;
        animator.SetFloat("Speed", speedParam);

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
    // 🚫 No crouch in vents
    if (inVent) return;

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
    // 🚫 No manual crawl toggle in vents – player stays in crawl
    if (inVent) return;

    if (!useToggleCrawl)
    {
        // hold-to-crawl
        if (ctx.performed)
        {
            ApplyStance(Stance.Crawl);
        }
        else if (ctx.canceled)
        {
            if (HasSpaceFor(standingHeight)) ApplyStance(Stance.Stand);
            else if (HasSpaceFor(crouchHeight)) ApplyStance(Stance.Crouch);
        }
        return;
    }

    // toggle behavior
    if (ctx.performed)
    {
        if (isCrawling)
        {
            if (HasSpaceFor(standingHeight)) ApplyStance(Stance.Stand);
            else if (HasSpaceFor(crouchHeight)) ApplyStance(Stance.Crouch);
        }
        else
        {
            ApplyStance(Stance.Crawl);
        }
    }
}


    public void ForceEnterCrawl()
    {
         var m = GetType().GetMethod("ApplyStance",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    // Stance.Crawl = 2
        m?.Invoke(this, new object[] { (object)2, true });
    }
    private void HandleFootsteps()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        // Horizontal speed
        Vector3 v = rb.linearVelocity;                 
        float horizSpeed = new Vector3(v.x, 0f, v.z).magnitude;

        // Only when grounded and actually moving
        if (!isGrounded || horizSpeed < footstepSpeedThreshold  || isCrawling)
        {
            // Decay accumulator a bit so quick taps don't instantly fire
            stepAccumulator = Mathf.Max(0f, stepAccumulator - Time.deltaTime);
            return;
        }

        // Determine stride/cadence multiplier by stance
          float strideMult = 1f;
    if (isCrouching) strideMult = crouchStrideMult;
    else if (isSprinting) strideMult = runStrideMult;

    float relSpeed = Mathf.Clamp(horizSpeed / Mathf.Max(0.01f, walkSpeed), 0f, 3f);
    float stepsPerSec = baseStepsPerSecond * relSpeed * strideMult;

    float interval = (stepsPerSec <= 0.01f) ? 999f : (1f / stepsPerSec);
    stepAccumulator += Time.deltaTime;

    if (stepAccumulator >= interval)
    {
        stepAccumulator -= interval;
        PlayFootstep();
    }
}

    private void PlayFootstep()
    {
        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        Vector3 pos = footstepOrigin ? footstepOrigin.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, pos, footstepVolume);
    }
    private void HandleLandingSfx()
    {
        if (landingClip == null) { wasGrounded = isGrounded; return; }

        if (!wasGrounded && isGrounded)
        {
            Vector3 pos = footstepOrigin ? footstepOrigin.position : transform.position;
            AudioSource.PlayClipAtPoint(landingClip, pos, landingVolume);
            stepAccumulator = 0f;
        }

        wasGrounded = isGrounded;
    }
public void ForceStand(bool force = false)
{
    var m = GetType().GetMethod("ApplyStance",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    // Stance.Stand = 0
    m?.Invoke(this, new object[] { (object)0, force });
}

    public void ForceCrouch(bool force = false)
    {
        var m = GetType().GetMethod("ApplyStance",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        // Stance.Crouch = 1
        m?.Invoke(this, new object[] { (object)1, force });
    }
    public void ExitVentUpright(bool forceStand = false)
{
    // If you want to always stand regardless of headroom (level design guarantees it), set forceStand = true
    var apply = GetType().GetMethod("ApplyStance",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

    // enum values in your script: Stand=0, Crouch=1, Crawl=2
    if (forceStand)
    {
        apply?.Invoke(this, new object[] { (object)0, /*force*/ true });
        return;
    }

    // Respect headroom:
    if (HasSpaceFor(standingHeight))
        apply?.Invoke(this, new object[] { (object)0, /*force*/ true }); // Stand
    else if (HasSpaceFor(crouchHeight))
        apply?.Invoke(this, new object[] { (object)1, /*force*/ true }); // Crouch
    else
        apply?.Invoke(this, new object[] { (object)2, /*force*/ true }); // stay Crawl if truly no space
}
public void SetInVent(bool value)
{
    inVent = value;

    if (inVent)
    {
        // When entering vents: force crawl & stop sprint
        ApplyStance(Stance.Crawl, true);
        isSprinting = false;
    }
    else
    {
        // When leaving vents you can choose what to do.
        // Right now we do nothing here because ExitVentUpright()
        // already sets the stance correctly from the portal script.
    }
}


   // internal void Teleport(Vector3 position, Quaternion rotation)
    //{
       // throw new System.NotImplementedException();
    //}
    #endregion
}
