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

    [Header("Cameras")]
    public Camera sceneCamera;   // normal scene camera
    public Camera panelCamera;   // camera that looks at world-space lose panel

    private void Start()
    {
        Time.timeScale = 1f;

        // Make sure lose panel is hidden at first
        if (losePanel != null)
            losePanel.SetActive(false);

        // Camera setup: start with scene camera ON, panel camera OFF
        if (sceneCamera != null)
            sceneCamera.enabled = true;
        if (panelCamera != null)
            panelCamera.enabled = false;

        if (anim == null)
            anim = GetComponent<Animator>();

        // start jumpscare animation
        if (anim != null && !string.IsNullOrEmpty(jumpScareTriggerName))
        {
            anim.SetTrigger(jumpScareTriggerName);
        }

        // flicker light
        if (flicker != null)
            flicker.StartFlicker();

        // camera shake
        if (cameraShake != null)
            cameraShake.StartShake(shakeDuration, shakeMagnitude);

        StartCoroutine(JumpScareSequence());
    }

    private IEnumerator JumpScareSequence()
    {
        // wait for jumpscare animation time
        yield return new WaitForSeconds(jumpScareDuration);

        // stop flicker
        if (flicker != null)
            flicker.StopFlicker();

        // switch cameras ➜ now use the panel camera
        if (sceneCamera != null)
            sceneCamera.enabled = false;
        if (panelCamera != null)
            panelCamera.enabled = true;

        // show lose panel
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (losePanel != null)
            losePanel.SetActive(true);

        // pause game
        Time.timeScale = 0f;
    }
}
