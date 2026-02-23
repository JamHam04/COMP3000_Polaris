using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

    // Menu Panels
    public GameObject MainPanel;
    public GameObject VRPanel;
    public GameObject GeneralPanel;
    public GameObject ConfirmationPanel;
    public TextMeshProUGUI confirmationText;

    private UnityAction pendingAction;



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

    // Menu panel management
    public void ShowVRPanel()
    {
        VRPanel.SetActive(true);
        MainPanel.SetActive(false);
        GeneralPanel.SetActive(false);
        ConfirmationPanel.SetActive(false);
    }

    public void ShowGeneralPanel()
    {
        MainPanel.SetActive(false);
        VRPanel.SetActive(false);
        GeneralPanel.SetActive(true);
        ConfirmationPanel.SetActive(false);
    }


    public void showMainPanel()
    {
        MainPanel.SetActive(true);
        VRPanel.SetActive(false);
        GeneralPanel.SetActive(false);
        ConfirmationPanel.SetActive(false);
    }

    public void ShowConfirmation(string message, UnityAction onConfirm)
    {
        confirmationText.text = message;
        pendingAction = onConfirm;
        ConfirmationPanel.SetActive(true);
        MainPanel.SetActive(false);
        VRPanel.SetActive(false);
        GeneralPanel.SetActive(false);
    }

    public void ConfirmAction()
    {
        pendingAction?.Invoke();
        ConfirmationPanel.SetActive(false);
    }

    public void CancelAction()
    {
        pendingAction = null;
        ConfirmationPanel.SetActive(false);
        MainPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

    }

    public void QuitGame()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ShowRestartConfirmation()
    {
        ShowConfirmation("Restart the level?", RestartLevel);
    }

    public void ShowQuitConfirmation()
    {
        ShowConfirmation("Return to main menu?", QuitGame);
    }

}
