using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Default Settings")]
    public float defaultDuration = 0.5f;
    public float defaultMagnitude = 0.3f;

    private Vector3 originalLocalPos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        originalLocalPos = transform.localPosition;
    }

    public void StartShake()
    {
        StartShake(defaultDuration, defaultMagnitude);
    }

    public void StartShake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // random offset around original position
            Vector3 offset = Random.insideUnitSphere * magnitude;
            offset.z = 0f; // keep it more like 2D shake, no depth wobble

            transform.localPosition = originalLocalPos + offset;

            yield return null;
        }

        // reset camera position
        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }
}
