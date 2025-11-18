// using UnityEngine;

// public class SimpleDoorController : MonoBehaviour
// {
//     [Header("References")]
//     public Transform player;       // اللاعب
//     public Transform doorMesh;     // المجسم الفعلي
//     public AudioSource audioSource; // مصدر الصوت
//     public AudioClip doorSound;     // الصوت الذي يُشغل عند فتح/إغلاق الباب

//     [Header("Settings")]
//     public float openDistance = 3f;
//     public float moveSpeed = 2f;
//     public Vector3 openOffset = new Vector3(2f, 0, 0);

//     private Vector3 closedPosition;
//     private Vector3 openPosition;
//     private bool isOpen;
//     private bool lastState; // لتتبع حالة الباب السابقة

//     void Start()
//     {
//         if (doorMesh == null)
//         {
//             Debug.LogError("❌ لم يتم تعيين المجسم (doorMesh) في SimpleDoorController!");
//             return;
//         }

//         closedPosition = doorMesh.position;
//         openPosition = closedPosition + openOffset;
//         lastState = false;
//     }

//     void Update()
//     {
//         if (player == null || doorMesh == null) return;

//         float distance = Vector3.Distance(player.position, transform.position);
//         isOpen = distance <= openDistance;

//         // تحريك الباب
//         Vector3 target = isOpen ? openPosition : closedPosition;
//         doorMesh.position = Vector3.MoveTowards(doorMesh.position, target, moveSpeed * Time.deltaTime);

//         // تشغيل الصوت عند تغيير حالة الباب
//         if (isOpen != lastState)
//         {
//             if (audioSource != null && doorSound != null)
//             {
//                 audioSource.PlayOneShot(doorSound);
//             }
//             lastState = isOpen;
//         }
//     }
// }


using UnityEngine;

public class SimpleDoorController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform doorMesh;
    public AudioSource audioSource;
    public AudioClip doorSound;

    [Header("Settings")]
    public float openDistance = 3f; // للمؤثرات الأخرى (اختياري)
    public float moveSpeed = 2f;
    public Vector3 openOffset = new Vector3(2f, 0, 0);
    public float fadeSpeed = 2f; // سرعة تلاشي الصوت
    public float maxVolumeDistance = 5f; // المسافة التي عندها الصوت أقوى

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 lastPosition;

    void Start()
    {
        if (doorMesh == null)
        {
            Debug.LogError("❌ لم يتم تعيين المجسم (doorMesh)!");
            return;
        }

        closedPosition = doorMesh.position;
        openPosition = closedPosition + openOffset;
        lastPosition = doorMesh.position;

        if (audioSource != null)
        {
            audioSource.clip = doorSound;
            audioSource.loop = true;
            audioSource.volume = 0f;
        }
    }

    void Update()
    {
        if (doorMesh == null || audioSource == null || doorSound == null) return;

        Vector3 target = Vector3.Distance(player.position, transform.position) <= openDistance ? openPosition : closedPosition;
        doorMesh.position = Vector3.MoveTowards(doorMesh.position, target, moveSpeed * Time.deltaTime);

        bool isMoving = Vector3.Distance(doorMesh.position, lastPosition) > 0.001f;

        if (isMoving)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            // تلاشي الصوت أثناء الحركة
            float distance = Vector3.Distance(player.position, transform.position);
            float targetVolume = Mathf.Clamp01(1f - distance / maxVolumeDistance); // أقوى صوت عند الاقتراب
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
        }
        else
        {
            // تلاشي الصوت عند توقف الحركة
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, 0f, fadeSpeed * Time.deltaTime);
            if (audioSource.volume <= 0.01f && audioSource.isPlaying)
                audioSource.Stop();
        }

        lastPosition = doorMesh.position;
    }
}
