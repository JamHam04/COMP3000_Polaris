using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    // Vignette settings
    public TunnelingVignetteController vignetteController;
    public Slider vignetteSlider;

    // Snap turn settings
    public ActionBasedSnapTurnProvider snapTurnProvider; 
    public Slider snapTurnSlider;
    public TextMeshProUGUI snapTurnValueText;

    public PlayerCollider playerCollider;
    // Seated toggle
    public Toggle seatedToggle;
    public CameraHeightOffset cameraHeightOffset;

    // General Settings
    // Volume
    public AudioMixer audioMixerController;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;


    // Start is called before the first frame update
    void Start()
    {
        if (vignetteController != null && vignetteSlider != null)
        {
            // Set initial slider value
            vignetteSlider.value = 1.0f - vignetteController.defaultParameters.apertureSize;
            // Add listener to update vignette when slider changes
            vignetteSlider.onValueChanged.AddListener(UpdateVignette);
        }

        if (snapTurnProvider != null && snapTurnSlider != null)
        {
            snapTurnSlider.value = snapTurnProvider.turnAmount / 45;
            snapTurnSlider.onValueChanged.AddListener(UpdateSnapTurn);
            if (snapTurnValueText != null)
            {
                snapTurnValueText.text = $"{snapTurnProvider.turnAmount}°";
            }
        }

        // Sitting mode toggle
        if (playerCollider != null && seatedToggle != null)
        {
            seatedToggle.onValueChanged.AddListener(ToggleSittingMode);
        }

        // Volume sliders
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(UpdateMasterVolume);
            UpdateMasterVolume(masterSlider.value);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
            UpdateMusicVolume(musicSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(UpdateSFXVolume);
            UpdateSFXVolume(sfxSlider.value);
        }
    }

    public void UpdateVignette(float value)
    {
        if (vignetteController != null)
        {
            vignetteController.defaultParameters.apertureSize = 1.0f - value;
        }
    }

    public void UpdateSnapTurn(float value)
    {
        if (snapTurnProvider != null)
        {
            float snapTurnValues = value * 45.0f;

            snapTurnProvider.turnAmount = snapTurnValues;

            if (snapTurnValueText != null)
            {
                snapTurnValueText.text = $"{snapTurnValues}°";
            }
        }
    }

    public void ToggleSittingMode(bool isSitting)
    {
        if (cameraHeightOffset != null)
            cameraHeightOffset.SetSeatedMode(isSitting);
    }

    // Volume control
    public void UpdateMasterVolume(float value)
    {
        if (audioMixerController != null)
        {
            // Convert 0–1 slider to dB (-80 dB = silence, 0 dB = full)
            float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixerController.SetFloat("Master", dB);
        }
    }

    public void UpdateMusicVolume(float value)
    {
        if (audioMixerController != null)
        {
            float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixerController.SetFloat("Music", dB);
        }
    }

    public void UpdateSFXVolume(float value)
    {
        if (audioMixerController != null)
        {
            float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixerController.SetFloat("SFX", dB);
        }
    }



}
