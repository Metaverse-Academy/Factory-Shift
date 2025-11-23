using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayLoopAudioOnEnter : MonoBehaviour
{
    [Header("Who triggers")]
    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("Options")]
    [Tooltip("If true, audio stops when player leaves the trigger.")]
    [SerializeField] private bool stopOnExit = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (audioSource == null || clip == null) return;

        // If already playing this same clip, do nothing
        if (audioSource.isPlaying && audioSource.clip == clip) return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!stopOnExit) return;

        if (audioSource != null && audioSource.isPlaying && audioSource.clip == clip)
        {
            audioSource.Stop();
        }
    }
}
