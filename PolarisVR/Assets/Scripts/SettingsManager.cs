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
    public static SettingsManager Instance;
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
    public Toggle muteToggle;
    private float previousMasterVolume = 1.0f;
    private bool isMuted = false;

    // Menu Panels
    public GameObject MainPanel;
    public GameObject VRPanel;
    public GameObject GeneralPanel;
    public GameObject ConfirmationPanel;
    public TextMeshProUGUI confirmationText;

    // Default values
    public float vignetteDefaultValue = 0.2f;
    public float snapTurnDefaultValue = 1.0f; 
    public float masterVolumeDefault = 1.0f;
    public float musicVolumeDefault = 1.0f;
    public float sfxVolumeDefault = 0.5f;
    public bool seatedModeDefault = false;
    public bool muteModeDefault = false;
    


    private UnityAction pendingAction;

    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reassign references after scene load
        if (vignetteController == null)
            vignetteController = FindObjectOfType<TunnelingVignetteController>();
        if (snapTurnProvider == null)
            snapTurnProvider = FindObjectOfType<ActionBasedSnapTurnProvider>();
        if (playerCollider == null)
            playerCollider = FindObjectOfType<PlayerCollider>();
        if (cameraHeightOffset == null)
            cameraHeightOffset = FindObjectOfType<CameraHeightOffset>();

        // UI references
        Canvas canvas = FindObjectOfType<Canvas>(true);

        MainPanel = canvas.transform.Find("MainPanel")?.gameObject;
        VRPanel = canvas.transform.Find("VRSettingsPanel")?.gameObject;
        GeneralPanel = canvas.transform.Find("GeneralSettingsPanel")?.gameObject;
        ConfirmationPanel = canvas.transform.Find("ConfirmationPanel")?.gameObject;

        vignetteSlider = VRPanel?.transform.Find("VignetteSlider")?.GetComponent<Slider>();
        snapTurnSlider = VRPanel?.transform.Find("SnapTurnSlider")?.GetComponent<Slider>();

        masterSlider = GeneralPanel?.transform.Find("MasterVolumeSlider")?.GetComponent<Slider>();
        musicSlider = GeneralPanel?.transform.Find("MusicVolumeSlider")?.GetComponent<Slider>();
        sfxSlider = GeneralPanel?.transform.Find("SfxVolumeSlider")?.GetComponent<Slider>();
        muteToggle = GeneralPanel?.transform.Find("MuteToggle")?.GetComponent<Toggle>();


        confirmationText = canvas.GetComponentInChildren<TextMeshProUGUI>(true);

        // Assign button listeners
        if (MainPanel != null)
        {
            Button vrSettingsButton = MainPanel.transform.Find("VRSettingsButton")?.GetComponent<Button>();
            Button generalSettingsButton = MainPanel.transform.Find("GeneralSettingsButton")?.GetComponent<Button>();
            Button restartButton = MainPanel.transform.Find("RestartLevelButton")?.GetComponent<Button>();
            Button quitButton = MainPanel.transform.Find("MenuButton")?.GetComponent<Button>();
            Button vrBackButton = VRPanel.transform.Find("BackButton")?.GetComponent<Button>();
            Button generalBackButton = GeneralPanel.transform.Find("BackButton")?.GetComponent<Button>();
            Button confirmButton = ConfirmationPanel.transform.Find("ConfirmButton")?.GetComponent<Button>();
            Button cancelButton = ConfirmationPanel.transform.Find("CancelButton")?.GetComponent<Button>();
            if (vrSettingsButton != null)
                vrSettingsButton.onClick.AddListener(ShowVRPanel);
            if (generalSettingsButton != null)
                generalSettingsButton.onClick.AddListener(ShowGeneralPanel);
            if (restartButton != null)
                restartButton.onClick.AddListener(ShowRestartConfirmation);
            if (quitButton != null)
                quitButton.onClick.AddListener(ShowQuitConfirmation);
            if (vrBackButton != null)
                vrBackButton.onClick.AddListener(showMainPanel);
            if (generalBackButton != null)
                generalBackButton.onClick.AddListener(showMainPanel);
            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmAction);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(CancelAction);
        }

        BindUIEvents();

        // Reapply settings
        ApplyValues();
    }

    void ApplyValues()
    {
        UpdateVignette(vignetteDefaultValue);
        UpdateSnapTurn(snapTurnDefaultValue);
        ToggleSittingMode(seatedModeDefault);
        UpdateMasterVolume(masterVolumeDefault);
        UpdateMusicVolume(musicVolumeDefault);
        UpdateSFXVolume(sfxVolumeDefault);

 

        // Update UI elements to reflect current settings
        if (vignetteSlider != null)
            vignetteSlider.SetValueWithoutNotify(vignetteDefaultValue);
        if (snapTurnSlider != null)
            snapTurnSlider.SetValueWithoutNotify(snapTurnDefaultValue);
        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(masterVolumeDefault);
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(musicVolumeDefault);
        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfxVolumeDefault);
        if (muteToggle != null)
            muteToggle.SetIsOnWithoutNotify(muteModeDefault);
        if (seatedToggle != null)
            seatedToggle.SetIsOnWithoutNotify(seatedModeDefault);
    }

    void BindUIEvents()
    {
        if (vignetteSlider != null)
            vignetteSlider.onValueChanged.AddListener(UpdateVignette);
        if (snapTurnSlider != null)
            snapTurnSlider.onValueChanged.AddListener(UpdateSnapTurn);
        if (seatedToggle != null)
            seatedToggle.onValueChanged.AddListener(ToggleSittingMode);
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(UpdateMasterVolume);
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(UpdateSFXVolume);
        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(ToggleMute);
    }

    // Start is called before the first frame update
    void Start()
    {
   

        if (vignetteSlider != null)
            vignetteSlider.onValueChanged.AddListener(UpdateVignette);

        if (snapTurnSlider != null)
            snapTurnSlider.onValueChanged.AddListener(UpdateSnapTurn);


        // Sitting mode toggle
        if (seatedToggle != null)
            seatedToggle.onValueChanged.AddListener(ToggleSittingMode);


        // Volume sliders


        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(UpdateMasterVolume);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(UpdateMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(UpdateSFXVolume);

        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(ToggleMute);
    }

    public void UpdateVignette(float value)
    {
        vignetteDefaultValue = value;
        if (vignetteController != null)
        {
            vignetteController.defaultParameters.apertureSize = 1.0f - value;
        }
    }

    public void UpdateSnapTurn(float value)
    {
        snapTurnDefaultValue = value;
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
        seatedModeDefault = isSitting;
        if (cameraHeightOffset != null)
            cameraHeightOffset.SetSeatedMode(isSitting);
    }

    // Volume control
    public void UpdateMasterVolume(float value)
    {


        masterVolumeDefault = value;

        if (isMuted)
        {
            isMuted = false;
            muteModeDefault = false;
            if (muteToggle != null)
            {
                muteToggle.SetIsOnWithoutNotify(false);
            }
        }

        if (audioMixerController != null)
        {
            // Convert 0–1 slider to dB (-80 dB = silence, 0 dB = full)
            float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixerController.SetFloat("Master", dB);
        }
    }

    public void UpdateMusicVolume(float value)
    {
        musicVolumeDefault = value;
        if (audioMixerController != null)
        {
            float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixerController.SetFloat("Music", dB);
        }
    }

    public void UpdateSFXVolume(float value)
    {
        sfxVolumeDefault = value;
        if (audioMixerController != null)
        {
            float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixerController.SetFloat("SFX", dB);
        }
    }

    public void ToggleMute(bool mute)
    {
        isMuted = mute;
        muteModeDefault = mute;


        if (isMuted)
        {
            previousMasterVolume = masterVolumeDefault;
            audioMixerController.SetFloat("Master", -80f);
        }
        else
        {
            UpdateMasterVolume(previousMasterVolume);
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
