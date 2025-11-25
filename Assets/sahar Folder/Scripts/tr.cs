using UnityEngine;

public class tr : MonoBehaviour
{
    public float cutsceneDuration = 51.36666f; 
    void Start()
    {
        Invoke(nameof(EndCutscene), cutsceneDuration);
    }

    [System.Obsolete]
    void EndCutscene()
{
    FindObjectOfType<CutsceneNight1Transition>().StartNight1Transition();
}

}