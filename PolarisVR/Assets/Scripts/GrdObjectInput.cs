using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class GrdObjectInput : MonoBehaviour
{
    
    private GridObject gridObject;
    private GridObject lastHoveredObject;
    public Transform playerTransform;


    // XR Toolkit button input
    public ActionBasedController xrController;
    public InputActionProperty magnetAction; // Set activate value
    public InputActionProperty climbAction; // Set climb value

    public XRRayInteractor xrRayInteractor;
    private XRInteractorLineVisual lineVisual;


    public float activationThreshold = 0.1f; // Threshold for button press
    private bool wasMagnetActivated= false; 
    private bool isMagnetActivated = false; 
    public float magnetCooldown = 0.5f; // Cooldown time between activations
    public float cooldownTimer = 0.0f;

    // Raycast
    public float raycastDistance = 10.0f; // To be used for magnet target
    public Transform rayOrigin;

    // Left or right hand controller
    public bool isLeftHand = false;
    public bool isRightHand = false;

    // Highlight timer (To reduce flickering)
    public float highlightEndDuration = 0.05f;
    private float highlightTimer = 0.0f;

    // Climbable faces
    private bool wasClimbActivated = false;
    private bool isClimbActivated = false;

    // Start is called before the first frame update
    void Start()
    {
        if (xrRayInteractor != null)
            lineVisual = xrRayInteractor.GetComponent<XRInteractorLineVisual>();

        if (lineVisual != null)
            lineVisual.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        updateHighlight();

        // VR input for magnet push/pull
        activateMagnetInput();
        activateClimbInput();

        if (cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
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

        if (magnetJustActivated && gridObject != null && playerTransform != null && cooldownTimer <= 0f)
        {
            // Check if magnet button is pressed
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastDistance))
            {
                if (lastHoveredObject != null && lastHoveredObject.canActivateMagnet(playerTransform, hit.normal, isLeftHand))
                {
                    Vector3Int dir = getActivationDirection(hit.normal);
                    if (isRightHand) dir = -dir; // Pull for left hand


                    Vector3Int targetCell = lastHoveredObject.currentCell + dir; // Target cell to move to
 
                    lastHoveredObject.MoveToCell(targetCell);
                    cooldownTimer = magnetCooldown; // Reset cooldown
                }
            }
        }
    }

    void activateClimbInput()
    {
        bool climbJustActivated = false;

        // Check if climb button is pressed
        float climbValue = climbAction.action.ReadValue<float>();

        if (climbValue > activationThreshold)
        {
            isClimbActivated = true;
        }
        else
        {
            isClimbActivated = false;
        }

        // Check if climb is activated again
        if (isClimbActivated && !wasClimbActivated)
        {
            climbJustActivated = true;
        }
        // Update previous state
        wasClimbActivated = isClimbActivated;

        if (climbJustActivated && gridObject != null && playerTransform != null && cooldownTimer <= 0f)
        {
            // Check if climb button is pressed
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastDistance))
            {
                if (lastHoveredObject != null && lastHoveredObject.canActivateMagnet(playerTransform, hit.normal, isLeftHand))
                {
                    Vector3Int dir = getActivationDirection(hit.normal);
                    if (lastHoveredObject.IsFaceClimbable(dir))
                    {
                        // Climb face
                        StartCoroutine(lastHoveredObject.ClimbFace(playerTransform));
                    }
                }
            }


        }
    }



    void updateHighlight()
    {
        RaycastHit hit;

        if (lineVisual != null)
            lineVisual.enabled = false;

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastDistance))
        {
            gridObject = hit.collider.gameObject.GetComponent<GridObject>();

            if (gridObject != null && gridObject.handType(isLeftHand))
            {
                Vector3 snappedFaceNormal = GridObject.SnapNormal(hit.normal);

                
                bool canActivate = gridObject.canActivateMagnet(playerTransform, snappedFaceNormal, isLeftHand);

                if (lineVisual != null)
                    lineVisual.enabled = true;

                // Clear previous highlight if hovering a new face
                if (lastHoveredObject != gridObject || (lastHoveredObject.highlightedFace - snappedFaceNormal).sqrMagnitude > 0.01f)
                {
                    if (lastHoveredObject != null)
                        lastHoveredObject.ClearHighlight();

                    lastHoveredObject = gridObject; // Update prevous hovered object
                }

                // Highlight new face
                gridObject.HighlightFace(hit, snappedFaceNormal, canActivate);

                highlightTimer = 0f; // Reset highlight timer
                return;
            }
        }

        // Clear highlight if no longer hovering
        if (lastHoveredObject != null)
        {
            highlightTimer += Time.deltaTime;
            if (highlightTimer >= highlightEndDuration) { 
                lastHoveredObject.ClearHighlight();
                lastHoveredObject = null;
                highlightTimer = 0f;
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

    Vector3Int getActivationDirection(Vector3 hitNormal)
    {
        // Determine direction based on hit normal

        // X axis
        if (Mathf.Abs(hitNormal.x) > 0.5f)
        {
            return new Vector3Int((int)hitNormal.x, 0, 0);
        }
        // Y axis
        else if (Mathf.Abs(hitNormal.y) > 0.5f)
        {
            return new Vector3Int(0, (int)hitNormal.y, 0);
        }
        // Z axis
        else
        {
            return new Vector3Int(0, 0, (int)hitNormal.z);
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
