using UnityEngine;

public class SimpleDoorController : MonoBehaviour
{
    [Header("References")]
    public Transform player;       // اللاعب
    public Transform doorMesh;     // المجسم الفعلي الذي يحتوي على الـMesh Renderer

    [Header("Settings")]
    public float openDistance = 3f;
    public float moveSpeed = 2f;
    public Vector3 openOffset = new Vector3(2f, 0, 0);

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;

    void Start()
    {
        if (doorMesh == null)
        {
            Debug.LogError("❌ لم يتم تعيين المجسم (doorMesh) في SimpleDoorController!");
            return;
        }

        closedPosition = doorMesh.position;
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        if (player == null || doorMesh == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        isOpen = distance <= openDistance;

        Vector3 target = isOpen ? openPosition : closedPosition;
        doorMesh.position = Vector3.MoveTowards(doorMesh.position, target, moveSpeed * Time.deltaTime);
    }
}
