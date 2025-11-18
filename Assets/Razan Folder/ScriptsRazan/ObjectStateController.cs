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

    [Header("Highlight State")]
    [SerializeField] private bool highlightEnabled = false;  // NEW

    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (baseMaterial != null)
            meshRenderer.material = baseMaterial;
    }

    void Update()
    {
        if (!highlightEnabled)               // NEW → if not active, keep base and skip
        {
            if (baseMaterial != null)
                meshRenderer.material = baseMaterial;
            return;
        }

        if (player == null) return;

        Vector3 dir = (transform.position - player.position).normalized;
        float rayDist = Vector3.Distance(player.position, transform.position);

        bool occluded = Physics.Raycast(
            player.position,
            dir,
            out RaycastHit hit,
            rayDist + 0.01f,
            obstructionMask
        );

        if (occluded && redMaterial != null)
        {
            meshRenderer.material = redMaterial;
        }
        else if (baseMaterial != null)
        {
            meshRenderer.material = baseMaterial;
        }
    }

    // استدعِ هذه الدالة لتفعيل/إلغاء الهايلايت من السكربت الآخر
    public void SetHighlightActive(bool active)
    {
        highlightEnabled = active;

        // رجّع الماتيريال الأساسي فوراً إذا أطفأناه
        if (!highlightEnabled && baseMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = baseMaterial;
        }
    }
}
