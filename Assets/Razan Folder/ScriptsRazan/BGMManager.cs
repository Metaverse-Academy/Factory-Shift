using UnityEngine;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // لا تدمر عند تحميل مشهد جديد
        }
        else
        {
            Destroy(gameObject); // إذا كان هناك نسخة أخرى، نحذفها لتفادي التكرار
        }
    }
}
