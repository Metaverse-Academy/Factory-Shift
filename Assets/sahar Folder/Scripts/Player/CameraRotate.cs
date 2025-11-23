using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CameraRotate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Rotation")]
    [Tooltip("Smoothly follow camera yaw. Disable for a hard lock.")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float turnSpeed = 10f;
    [Tooltip("Add an offset if your mesh faces a different forward (+Z).")]
    [SerializeField] private float yawOffset = 0f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        cameraTransform.SetParent(null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        if (!cameraTransform) return;

        HandleRotation();
    }

    private void HandleRotation()
    {
        Vector3 flatFwd = cameraTransform.forward; flatFwd.y = 0f;
        if (flatFwd.sqrMagnitude < 1e-6f) return;
        flatFwd.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(Quaternion.Euler(0f, yawOffset, 0f) * flatFwd, Vector3.up);

        if (smoothFollow)
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        else
            rb.MoveRotation(targetRot);
    }
}
