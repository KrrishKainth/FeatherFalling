using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    public AudioMixer mixer;
    Slider slider;
    public string sliderType;

    void Start()
    {
        slider = GetComponent<Slider>();

        // Update slider to current volume value
        float volume;
        if (sliderType == "master")
        {
            mixer.GetFloat("MasterVolume", out volume);
        }
        else if (sliderType == "music")
        {
            mixer.GetFloat("MusicVolume", out volume);
        }
        else
        {
            mixer.GetFloat("SFXVolume", out volume);
        }

        slider.value = volumeToSliderVal(volume);
    }

    public void SetMasterVolume()
    {
        mixer.SetFloat("MasterVolume", sliderValToVolume(slider.value));
    }

    public void SetMusicVolume()
    {
        mixer.SetFloat("MusicVolume", sliderValToVolume(slider.value));
    }

    public void SetSFXVolume()
    {
        mixer.SetFloat("SFXVolume", sliderValToVolume(slider.value));
    }

    // Slider value mapped from 0 - 1 to -20 dB - 20 dB
    float sliderValToVolume(float sliderVal)
    {
        // return Mathf.Log(sliderVal * (Mathf.Pow(10, 2) - Mathf.Pow(10, -8)) + Mathf.Pow(10, -8));
        return sliderVal * 40 - 20;
    }

    float volumeToSliderVal(float volume)
    {
        // return (Mathf.Pow(10, volume) - Mathf.Pow(10, -8)) / (Mathf.Pow(10, 2) - Mathf.Pow(10, -8));
        return (volume + 20) / 40;
    }
}
