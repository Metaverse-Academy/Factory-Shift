using System.Collections;
using UnityEngine;

public class JumpScareController : MonoBehaviour
{
    [Header("Animation")]
    public Animator anim;
    public string jumpScareTriggerName = "Play";
    public float jumpScareDuration = 2.5f;

    [Header("UI")]
    public GameObject losePanel;

    [Header("Light Flicker")]
    public LightFlicker flicker;

    [Header("Camera Shake")]
    public CameraShake cameraShake;
    public float shakeDuration = 1.0f;
    public float shakeMagnitude = 0.4f;

    private void Start()
    {
        Time.timeScale = 1f;

        if (losePanel != null)
            losePanel.SetActive(false);

        if (anim == null)
            anim = GetComponent<Animator>();

        // start jumpscare animation
        if (anim != null && !string.IsNullOrEmpty(jumpScareTriggerName))
        {
            anim.SetTrigger(jumpScareTriggerName);
        }

        // start spotlight flicker
        if (flicker != null)
            flicker.StartFlicker();

        // start camera shake
        if (cameraShake != null)
            cameraShake.StartShake(shakeDuration, shakeMagnitude);

        StartCoroutine(JumpScareSequence());
    }

    private IEnumerator JumpScareSequence()
    {
        // wait for the jumpscare animation
        yield return new WaitForSeconds(jumpScareDuration);

        // stop flicker
        if (flicker != null)
            flicker.StopFlicker();

        if (losePanel != null)
            losePanel.SetActive(true);

        Time.timeScale = 0f;
    }
}
