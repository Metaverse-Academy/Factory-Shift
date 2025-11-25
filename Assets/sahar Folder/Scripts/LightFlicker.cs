using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Target Light")]
    public Light targetLight;   // drag your spotlight here

    [Header("Flicker Settings")]
    public float minInterval = 0.02f;  // fastest flicker
    public float maxInterval = 0.08f;  // slowest flicker
    public bool startOnAwake = false;

    private bool isFlickering;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        if (startOnAwake)
            StartFlicker();
    }

    public void StartFlicker()
    {
        if (isFlickering) return;
        isFlickering = true;
        StartCoroutine(FlickerRoutine());
    }

    public void StopFlicker()
    {
        isFlickering = false;
        // optional: make sure the light stays ON at the end
        if (targetLight != null)
            targetLight.enabled = true;
    }

    private IEnumerator FlickerRoutine()
    {
        if (targetLight == null) yield break;

        while (isFlickering)
        {
            // toggle on/off
            targetLight.enabled = !targetLight.enabled;

            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);
        }
    }
}
