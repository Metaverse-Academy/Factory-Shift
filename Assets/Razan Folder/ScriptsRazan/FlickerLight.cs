using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public Light targetLight;        // الضوء الذي نريد التحكم فيه
    public float minIntensity = 0f;  // أقل شدة للضوء
    public float maxIntensity = 1f;  // أعلى شدة للضوء
    public float flickerSpeed = 0.1f; // سرعة الوميض

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= flickerSpeed)
        {
            // تغيير شدة الضوء بشكل عشوائي بين min و max
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);
            timer = 0f;
        }
    }
}
