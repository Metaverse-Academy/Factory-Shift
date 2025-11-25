using UnityEngine;

public class CloseCreditsOnClick : MonoBehaviour
{
    void Update()
    {
        // لو اللاعب ضغط أي زر بالماوس أو باللمس
        if (Input.GetMouseButtonDown(0))
        {
            gameObject.SetActive(false);
        }
    }
}
