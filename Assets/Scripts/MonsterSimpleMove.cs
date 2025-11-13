using UnityEngine;

public class MonsterSimpleMove : MonoBehaviour
{
    [Header("Move Settings")]
    public float speed = 2f;
    public Transform stopPoint;

    [HideInInspector] public bool shouldMove = false;

    [Header("Animation")]
    public Animator anim;
    [Tooltip("Name of the bool parameter in the Animator")]
    public string walkBoolName = "IsWalking";

    private void Start()
    {
        // Auto–get Animator if not set in Inspector
        if (anim == null)
            anim = GetComponent<Animator>();

        SetWalking(false);
    }

    private void Update()
    {
        if (!shouldMove)
        {
            SetWalking(false);
            return;
        }

        SetWalking(true);

        // No stop point → walk forward forever
        if (stopPoint == null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }

        // Move toward stop point
        transform.position = Vector3.MoveTowards(
            transform.position,
            stopPoint.position,
            speed * Time.deltaTime
        );

        // Face the target (optional)
        Vector3 dir = stopPoint.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // Stop when close enough
        if (Vector3.Distance(transform.position, stopPoint.position) < 0.05f)
        {
            shouldMove = false;
            SetWalking(false);
        }
    }

    private void SetWalking(bool walking)
    {
        if (anim == null) return;
        anim.SetBool(walkBoolName, walking);
    }
}
