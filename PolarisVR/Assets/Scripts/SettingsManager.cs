using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SettingsManager : MonoBehaviour
{
    public TunnelingVignetteController vignetteController;
    public Slider vignetteSlider;

    public ActionBasedSnapTurnProvider snapTurnProvider; 
    public Slider snapTurnSlider;

    public PlayerCollider playerCollider;
    // Seated toggle
    public Toggle seatedToggle;
    public CameraHeightOffset cameraHeightOffset;


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
        }

        // Sitting mode toggle
        if (playerCollider != null && seatedToggle != null)
        {
            seatedToggle.onValueChanged.AddListener(ToggleSittingMode);
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
        }
    }

    public void ToggleSittingMode(bool isSitting)
    {
        if (cameraHeightOffset != null)
            cameraHeightOffset.SetSeatedMode(isSitting);
    }
}
