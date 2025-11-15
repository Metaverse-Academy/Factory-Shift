using UnityEngine;
using UnityEngine.Audio;

public class MixerControl : MonoBehaviour
{
    public AudioMixer mixer;

    // volume بين 0 و 1
    public void SetVolume(float volume)
    {
        mixer.SetFloat("MusicVolume", Mathf.Lerp(-80f, 0f, volume));
    }
}
