using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ObjectStateController : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // Transform اللاعب/الكاميرا
    public LayerMask obstructionMask;        // طبقة الحائط (مثلاً "Wall")
    
    [Header("Materials")]
    public Material baseMaterial;            // المادة العادية
    public Material redMaterial;             // عندما يكون مخفي خلف الحائط

    // داخلياً
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // ضبط المادة الأساسية عند البداية
        if (baseMaterial != null)
            meshRenderer.material = baseMaterial;
    }

    void Update()
    {
        if (player == null) return;

        // اتجاه ومسافة الجسم من اللاعب
        Vector3 dir = (transform.position - player.position).normalized;
        float rayDist = Vector3.Distance(player.position, transform.position);

        // اكتشاف إذا كان هناك عائق بين اللاعب والجسم
        bool occluded = Physics.Raycast(player.position, dir, out RaycastHit hit, rayDist + 0.01f, obstructionMask);

        // تغيير المادة فقط إذا الجسم خلف الحائط
        if (occluded && redMaterial != null)
        {
            meshRenderer.material = redMaterial;
        }
        else if (baseMaterial != null)
        {
            meshRenderer.material = baseMaterial;
        }
    }
}
