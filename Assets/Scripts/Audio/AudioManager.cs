using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider BGM_slider;
    [SerializeField] Slider SE_slider;
    [SerializeField] AnimationCurve ease_audio;

    private void Start()
    {
        //BGM
        audioMixer.GetFloat("BGM", out float BGM_volume);
        BGM_slider.value = BGM_volume;
        //SE
        audioMixer.GetFloat("SE", out float SE_volume);
        SE_slider.value = SE_volume;
    }

    public void SetBGM(float volume)
    {
        var t_volume = ease_audio.Evaluate(volume);
        audioMixer.SetFloat("BGM", t_volume);
    }

    public void SetSE(float volume)
    {
        audioMixer.SetFloat("SE", volume);
    }
}