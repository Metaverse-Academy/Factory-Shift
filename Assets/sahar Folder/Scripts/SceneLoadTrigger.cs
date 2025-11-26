using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneLoadAfterRepairs : MonoBehaviour
{
    [Header("Who can trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Required Repairs")]
    [Tooltip("Fan 1 RepairableFixable script")]
    [SerializeField] private RepairableFixable fan1;

    [Tooltip("Fan 2 RepairableFixable script")]
    [SerializeField] private RepairableFixable fan2;

    [Header("Scene")]
    [Tooltip("Exact scene name from Build Settings.")]
    [SerializeField] private string sceneToLoad;

    [Header("Options")]
    [Tooltip("If true, loads only once.")]
    [SerializeField] private bool onlyOnce = true;
   

    private bool hasLoaded;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (onlyOnce && hasLoaded) return;

        // ⚠️ if fans not assigned, just warn
        if (fan1 == null || fan2 == null)
        {
            Debug.LogWarning("SceneLoadAfterRepairs: fan references not set.");
            return;
        }

        // Check if both repaired
        if (fan1.IsRepaired && fan2.IsRepaired)
        {
            LoadScene();
        }
    }

    private void LoadScene()
    {
        if (onlyOnce)
            hasLoaded = true;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("SceneLoadAfterRepairs: sceneToLoad is empty!");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }
}
