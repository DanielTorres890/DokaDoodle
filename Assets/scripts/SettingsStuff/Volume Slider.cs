using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public TextMeshProUGUI m_TextMeshProUGUI;
    public Slider m_Slider;
    public SettingsSliders type;

    void Start()
    {
        if(type == SettingsSliders.BGM) { m_Slider.value = SettingsManager.instance.volume * 100; m_TextMeshProUGUI.text = (Mathf.Round(SettingsManager.instance.volume * 100)).ToString(); }
        if(type == SettingsSliders.SFX) { m_Slider.value = SettingsManager.instance.SFXVolume * 100; m_TextMeshProUGUI.text = (Mathf.Round(SettingsManager.instance.SFXVolume * 100)).ToString(); }
        if(type == SettingsSliders.Sensitivity) { m_Slider.value = SettingsManager.instance.mouseSense; m_TextMeshProUGUI.text = (Mathf.Round(SettingsManager.instance.mouseSense * 100) / 100).ToString(); }
        
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
    public void SensitivityChanged(float value)
    {
        SettingsManager.instance.mouseSense = value;
        m_TextMeshProUGUI.text = (Mathf.Round(SettingsManager.instance.mouseSense * 100) / 100).ToString();
    }
}
public enum SettingsSliders{
    BGM,
    SFX,
    Sensitivity
}