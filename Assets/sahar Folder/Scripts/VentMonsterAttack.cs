using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;   // ⬅ important

public class VentMonsterAttack : MonoBehaviour
{
    [Header("Chase Settings")]
    public float crawlSpeed = 7f;
    public float attackDistance = 1.5f; // how close before we switch scene

    [Header("References")]
    public Transform player;                // drag Player here
    public Animator anim;                   // monster Animator
    public string crawlBoolName = "IsCrawling";

    [Header("Player Control")]
    public MonoBehaviour playerMovement;    // drag your player movement script here

    [Header("Jump Scare Scene")]
    [Tooltip("Exact name of the jumpscare scene (from Build Settings).")]
    public string jumpScareSceneName = "JumpScareScene";
    [Header("Vent Spawn Audio")]
    public AudioSource ventAudioSource;  
    public AudioClip spawnClip;          
    [Range(0f, 1f)] public float spawnVolume = 1f;


    private bool isChasing = false;
    private bool hasTriggeredScene = false;

    private void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        SetCrawling(false);
        // You can keep this object disabled in the scene and enable it only on fail.
    }

    private void Update()
    {
        if (!isChasing || player == null || hasTriggeredScene)
            return;

        // Direction to player (keep flat)
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        // Face player
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        // Crawl forward
        transform.position += transform.forward * crawlSpeed * Time.deltaTime;

        // When close enough → trigger jumpscare scene
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackDistance)
        {
            TriggerJumpScareScene();
        }
    }

    private void TriggerJumpScareScene()
    {
        if (hasTriggeredScene) return;
        hasTriggeredScene = true;
        isChasing = false;

        SetCrawling(false);

        // Stop player movement so they can't run away
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Make sure time is normal before switching scene
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(jumpScareSceneName))
        {
            SceneManager.LoadScene(jumpScareSceneName);
        }
        else
        {
            Debug.LogError("VentMonsterAttack: jumpScareSceneName is empty!");
        }
    }

    private void SetCrawling(bool value)
    {
        if (anim != null && !string.IsNullOrEmpty(crawlBoolName))
        {
            anim.SetBool(crawlBoolName, value);
        }
    }

    /// <summary>
    /// Called from RepairableFixable when the skill check FAILS.
    /// </summary>
    public void StartVentAttack(Transform targetPlayer)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        player = targetPlayer;
        hasTriggeredScene = false;
        isChasing = true;

        SetCrawling(true); 
        PlaySpawnSound();
    }
    private void PlaySpawnSound()
{
    if (ventAudioSource != null && spawnClip != null)
    {
        ventAudioSource.PlayOneShot(spawnClip, spawnVolume);
    }
}

}
