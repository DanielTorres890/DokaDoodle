using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public TextMeshProUGUI m_TextMeshProUGUI;
    public Slider m_Slider;
    public VolumeSliderTypes type;

    void Start()
    {
        if(type == VolumeSliderTypes.BGM) { m_Slider.value = SettingsManager.instance.volume * 100; }
        if(type == VolumeSliderTypes.SFX) { m_Slider.value = SettingsManager.instance.SFXVolume * 100; }
        
    }

    public void volumeChanged(float value)
    {
        SettingsManager.instance.ChangeBGMVolume(Mathf.RoundToInt(value));
        m_TextMeshProUGUI.text = value.ToString();
    }

    public void SFXVolumeChanged(float value)
    {
        SettingsManager.instance.ChangeSFXVolume(Mathf.RoundToInt(value));
        m_TextMeshProUGUI.text = value.ToString();
    }
}
public enum VolumeSliderTypes{
    BGM,
    SFX
}