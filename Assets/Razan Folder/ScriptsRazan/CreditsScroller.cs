// using UnityEngine;

// public class CreditsScroller : MonoBehaviour
// {
//     public float speed = 50f;   // سرعة الحركة للأعلى

//     void Update()
//     {
//         transform.Translate(Vector3.up * speed * Time.deltaTime);
//     }
// }

using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    public float speed = 50f;          
    public Vector3 startPosition;       
    public float endY = 1000f;          
    public GameObject creditsCanvas;    // الكانفس الرئيسي

    private void Awake()
    {
        if (startPosition == Vector3.zero)
            startPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        transform.localPosition = startPosition;
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (transform.localPosition.y >= endY)
        {
            // الآن نعطل الكانفس بدل النص نفسه
            creditsCanvas.SetActive(false);
        }
    }
}

