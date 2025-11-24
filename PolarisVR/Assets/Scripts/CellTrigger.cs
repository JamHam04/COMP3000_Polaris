using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellTrigger : MonoBehaviour
{
    // Trigger cell
    public Vector3Int triggerCell;

    // Link door 
    public Door triggeredDoor;

    // Trigger state
    private bool isTriggerActive = false;
    private GridController gridController;

    public Renderer frameRenderer; 

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
        if (gridController.IsCellOccupied(triggerCell))
        {
            if (!isTriggerActive)
            {
                // Activate linked dI oor
                isTriggerActive = true;
                triggeredDoor.OpenDoor();
                Debug.Log("Trigger activated");
                SetFrameEmission(true);
            }
        }
        else
        {
            if (isTriggerActive)
            {
                // Deactivate linked door
                triggeredDoor.CloseDoor();
                isTriggerActive = false;
                Debug.Log("Trigger deactivated");
                SetFrameEmission(false);
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
