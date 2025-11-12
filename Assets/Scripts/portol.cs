using UnityEngine;

public class portol : MonoBehaviour
{
    public enum Mode { EnterVent, ExitVent }

    [SerializeField] private Transform destination;

    [Header("Behavior")]
    [SerializeField] private Mode mode = Mode.EnterVent;
    [Tooltip("Prevents bouncing back instantly when you arrive inside the other trigger.")]
    [SerializeField] private float reenterLockout = 0.25f;
    private static readonly System.Collections.Generic.Dictionary<int, float> lockoutUntil
        = new System.Collections.Generic.Dictionary<int, float>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        
        var root = other.transform.root;
        if (!root.TryGetComponent<PlayerMovement>(out var player)) return;

        int id = root.GetInstanceID();
        if (lockoutUntil.TryGetValue(id, out float until) && Time.time < until) return;

        
        player.Teleport(destination.position, destination.rotation);

       
        if (mode == Mode.EnterVent)
        {
            
            player.ForceEnterCrawl();
        }
        else // ExitVent
        {
            
            player.ForceStand(); 
        
        }

        lockoutUntil[id] = Time.time + reenterLockout;
    }

    void OnDrawGizmos()
    {
        if (!destination) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(destination.position, 0.25f);
        Gizmos.DrawRay(destination.position, destination.forward * 0.6f);
    }
}
