using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneAutoGoToScene : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "NextLevelScene";
    [SerializeField] private float fadeOut = 0.6f;
    [SerializeField] private float fadeIn  = 0.6f;

    private PlayableDirector director;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += OnStopped;
    }
    private void OnDestroy()
    {
        if (director) director.stopped -= OnStopped;
    }
    private void Start()
    {
        director.time = 0;
        director.Play();
    }
    private void OnStopped(PlayableDirector _)
    {
        if (GlobalScreenFader.Instance != null)
            GlobalScreenFader.Instance.LoadSceneWithFade(targetSceneName, fadeOut, fadeIn);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
    }
}
