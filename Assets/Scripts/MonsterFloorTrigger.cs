using UnityEngine;

public class MonsterFloorTrigger : MonoBehaviour
{
    public MonsterSimpleMove monster;
    public bool oneShot = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && hasTriggered) return;

        hasTriggered = true;

        if (monster != null)
        {
            monster.shouldMove = true;
        }
    }
}
