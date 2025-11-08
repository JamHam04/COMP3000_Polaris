using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrdObjectInput : MonoBehaviour
{
    
    private GridObject gridObject;
    public ActionBasedController xrController;
    

    // XR Toolkit button input
    public InputHelpers.Button magnetButton = InputHelpers.Button.Trigger; // Button to activate magnet
    public float activationThreshold = 0.1f; // Threshold for button press

    // Raycast
    public float raycastDistance = 10.0f; // To be used for magnet target


    // Start is called before the first frame update
    void Start()
    {
        // Get GridController script
        gridObject = FindObjectOfType<GridObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // Movement pc test input 
        testInput();

        // VR input for magnet push/pull
        activateMagnetInput();
    }

    void testInput()
    {

        Vector3Int targetCell = gridObject.currentCell;

        if (Input.GetKeyDown(KeyCode.W))
        {
            targetCell += Vector3Int.forward;
            gridObject.MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            targetCell += Vector3Int.back;
            gridObject.MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            targetCell += Vector3Int.left;
            gridObject.MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            targetCell += Vector3Int.right;
            gridObject.MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            targetCell += Vector3Int.up;
            gridObject.MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            targetCell += Vector3Int.down;
            gridObject.MoveToCell(targetCell);
        }

    }

    void activateMagnetInput()
    {

        if (xrController)
        {
           
            Vector3Int targetCell = gridObject.currentCell;
            targetCell += Vector3Int.forward;
            gridObject.MoveToCell(targetCell);
        
        }
    }


}
