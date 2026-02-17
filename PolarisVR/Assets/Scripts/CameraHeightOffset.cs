using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHeightOffset : MonoBehaviour
{

    public float seatedHeightOffset = 0.5f;

    private bool isSeated = false;

    public void SetSeatedMode(bool seated)
    {
        isSeated = seated;
        UpdateCameraHeight();
    }

    void UpdateCameraHeight()
    {
        float offsetY = isSeated ? seatedHeightOffset : 0f;
        transform.localPosition = new Vector3(transform.localPosition.x, offsetY, transform.localPosition.z);
    }
}
