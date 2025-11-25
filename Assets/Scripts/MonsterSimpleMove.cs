using UnityEngine;

public class MonsterSimpleMove : MonoBehaviour
{
    [Header("Move Settings")]
    public float speed = 2f;
    public Transform stopPoint;

    [HideInInspector] public bool shouldMove = false;

    [Header("Animation")]
    public Animator anim;
    public string walkBoolName = "IsWalking";

    [Header("Audio")]
    [Tooltip("AudioSource for monster SFX (roar/step/etc.).")]
    public AudioSource monsterSfxSource;
    public AudioClip startWalkSfx;

    [Tooltip("AudioSource for scary music sting.")]
    public AudioSource scareMusicSource;
    public AudioClip scareMusicClip;
     [Header("Player Scare Audio")]
    [Tooltip("Player's AudioSource for scared breathing.")]
    public AudioSource playerBreathSource;
    public AudioClip scaredBreathClip;

    private bool audioPlayed = false; // make sure it only plays once

    private void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        SetWalking(false);
    }

    private void Update()
    {
        if (!shouldMove)
        {
            SetWalking(false);
            return;
        }

        SetWalking(true);

        // No stop point → walk forward forever
        if (stopPoint == null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }

        // Move toward stop point
        transform.position = Vector3.MoveTowards(
            transform.position,
            stopPoint.position,
            speed * Time.deltaTime
        );

        // Face target
        Vector3 dir = stopPoint.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // Stop when close enough
        if (Vector3.Distance(transform.position, stopPoint.position) < 0.05f)
        {
            shouldMove = false;
            SetWalking(false);
        }
    }

    private void SetWalking(bool walking)
    {
        // Animation
        if (anim != null)
        {
            anim.SetBool(walkBoolName, walking);
        }

        // Audio: when walking starts the first time → play sounds
        if (walking && !audioPlayed)
        {
            PlayStartAudio();
            audioPlayed = true;
        }
    }

    private void PlayStartAudio()
    {
        // Monster SFX
        if (monsterSfxSource != null && startWalkSfx != null)
        {
            monsterSfxSource.PlayOneShot(startWalkSfx);
        }

        // Scary sting
        if (scareMusicSource != null && scareMusicClip != null)
        {
            scareMusicSource.PlayOneShot(scareMusicClip);
            // or: scareMusicSource.clip = scareMusicClip;
            // scareMusicSource.Play();
        }
        // Player scared breathing
        if (playerBreathSource != null && scaredBreathClip != null)
        {
            playerBreathSource.PlayOneShot(scaredBreathClip);
        }
    }

    }

