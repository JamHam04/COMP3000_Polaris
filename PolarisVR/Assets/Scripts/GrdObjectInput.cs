using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class GrdObjectInput : MonoBehaviour
{
    
    private GridObject gridObject;



    // XR Toolkit button input
    public ActionBasedController xrController;
    public InputActionProperty magnetAction; // Set activate value

    public float activationThreshold = 0.1f; // Threshold for button press
    private bool wasMagnetActivated= false; 
    private bool isMagnetActivated = false; 

    // Raycast
    public float raycastDistance = 10.0f; // To be used for magnet target
    public Transform rayOrigin;
   


    // Left or right hand controller
    public bool isLeftHand = false;
    public bool isRightHand = false;

    // 



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Movement pc test input 
        //testInput();

        // VR input for magnet push/pull
        activateMagnetInput();

        //Debug.Log(xrController.transform.forward);

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

        bool magnetJustActivated = false;

        // Check if magnet button is pressed
        float magnetValue = magnetAction.action.ReadValue<float>();

        if (magnetValue > activationThreshold)
        {
            isMagnetActivated = true;
        }
        else
        {
            isMagnetActivated = false;
        }

        // Check if magnet is activated again 
        if (isMagnetActivated && !wasMagnetActivated)
        {
            magnetJustActivated = true;
        }
        // Update previous state
        wasMagnetActivated = isMagnetActivated;


        // Cast ray from controller
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastDistance))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
            gridObject = hit.collider.gameObject.GetComponent<GridObject>();

            if (gridObject != null)
            {
                // Check if magnet button is pressed
                
                if (magnetJustActivated)
                {
                    Vector3Int dir = getControllerDirection();
                    if (isLeftHand) dir = -dir; // Pull for left hand
                    gridObject.MoveToCell(gridObject.currentCell + dir);
                }
            }

         }
    }

    // Get direction from controller
    Vector3Int getControllerDirection()
    {
        // Use controller rayOrigin forward direction 
        Vector3 forward = rayOrigin.forward;

        // Get primary direction
        float absX = Mathf.Abs(forward.x);
        float absY = Mathf.Abs(forward.y);
        float absZ = Mathf.Abs(forward.z);

        // Return direction as Vector3Int
        if (absX > absY && absX > absZ)
        {
            return new Vector3Int(forward.x > 0 ? 1 : -1, 0, 0);
        }
        else if (absY > absX && absY > absZ)
        {
            return new Vector3Int(0, forward.y > 0 ? 1 : -1, 0);
        }
        else
        {
            return new Vector3Int(0, 0, forward.z > 0 ? 1 : -1);
        }


    }

    // Test raycasrt
    void OnDrawGizmos()
    {
        if (xrController)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * raycastDistance);
        }
    }



}
