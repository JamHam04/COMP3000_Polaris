using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellTrigger : MonoBehaviour
{
    // Trigger cell
    public Vector3Int triggerCell;

    // Trigger state
    private bool isTriggerActive = false;
    public bool IsTriggerActive => isTriggerActive;

    // Grid
    private GridController gridController;
    private GridObject activeCube;
    

    public Renderer frameRenderer;
    
    // Trigger type
    public CubeType triggerType;

    // Audio
    public AudioSource triggerAudio;
    public AudioClip triggerSound;


    // Start is called before the first frame update
    void Start()
    {
        // Get GridController script
        gridController = FindObjectOfType<GridController>();

        // Get Renderer for frame
        frameRenderer = GetComponent<Renderer>();

        // Initialize current cell
        triggerCell = gridController.worldToCell(transform.position);
        transform.position = gridController.CellToWorld(triggerCell);
    }

    // Update is called once per frame
    void Update()
    {

        // Get cube in trigger cell
        GridObject cubeInCell = gridController.GetCubeInCell(triggerCell);

        bool triggerActivated = (cubeInCell != null && cubeInCell.cubeType == triggerType);

        // Check if cube type matches trigger type
        if (triggerActivated != isTriggerActive)
        {
            
            isTriggerActive = triggerActivated;
            SetFrameEmission(triggerActivated);

            if (activeCube != null)
            {
                activeCube.SetEmission(false);
            }
            if (isTriggerActive)
            {
                activeCube = cubeInCell;
                // Play sound effect
                if (triggerAudio != null && triggerSound != null)
                {
                    triggerAudio.PlayOneShot(triggerSound);
                }
            }
            else
            {
                activeCube = null;
            }
            
        }
    }


    // Enable and disable emission on frame
    public void SetFrameEmission(bool enableGlow)
    {
        if (enableGlow)
        {
            frameRenderer.material.EnableKeyword("_EMISSION"); // Enable emission
        }
        else
        {
            frameRenderer.material.DisableKeyword("_EMISSION"); // Diable emission
        }
    }


}
