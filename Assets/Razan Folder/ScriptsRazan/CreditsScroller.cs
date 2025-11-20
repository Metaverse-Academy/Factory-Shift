using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    public float speed = 50f;   // سرعة الحركة للأعلى

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
}
