using UnityEngine;

public class SimpleDoorController : MonoBehaviour
{
    [Header("References")]
    public Transform player;       // اللاعب
    public Transform doorMesh;     // المجسم الفعلي
    public AudioSource audioSource; // مصدر الصوت
    public AudioClip doorSound;     // الصوت الذي يُشغل عند فتح/إغلاق الباب

    [Header("Settings")]
    public float openDistance = 3f;
    public float moveSpeed = 2f;
    public Vector3 openOffset = new Vector3(2f, 0, 0);

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;
    private bool lastState; // لتتبع حالة الباب السابقة

    void Start()
    {
        if (doorMesh == null)
        {
            Debug.LogError("❌ لم يتم تعيين المجسم (doorMesh) في SimpleDoorController!");
            return;
        }

        closedPosition = doorMesh.position;
        openPosition = closedPosition + openOffset;
        lastState = false;
    }

    void Update()
    {
        if (player == null || doorMesh == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        isOpen = distance <= openDistance;

        // تحريك الباب
        Vector3 target = isOpen ? openPosition : closedPosition;
        doorMesh.position = Vector3.MoveTowards(doorMesh.position, target, moveSpeed * Time.deltaTime);

        // تشغيل الصوت عند تغيير حالة الباب
        if (isOpen != lastState)
        {
            if (audioSource != null && doorSound != null)
            {
                audioSource.PlayOneShot(doorSound);
            }
            lastState = isOpen;
        }
    }
}
