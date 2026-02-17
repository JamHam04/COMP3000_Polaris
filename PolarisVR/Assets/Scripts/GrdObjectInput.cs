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
    public InputActionProperty menuAction; // Set menu value

    public XRRayInteractor xrRayInteractor;
    private XRInteractorLineVisual lineVisual;


    public float activationThreshold = 0.1f; // Threshold for button press
    private bool wasMagnetActivated= false; 
    private bool isMagnetActivated = false; 
    public float magnetCooldown = 0.5f; // Cooldown time between activations
    public float cooldownTimer = 0.0f;
    public PlayerDoorCheck playerDoorCheck;

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
    private float maxClimbDistance = 1.5f; // Max distance player can climb from

    // Player Menu
    public bool menuActive = false;
    public GameObject menuCanvas;

    // Audio 
    public AudioSource handAudio;

    public AudioClip highlightValidClip;
    public AudioClip highlightInvalidClip;
    public AudioClip activateClip;
    public AudioClip invalidActivateClip;

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

        // Menu input
        if (menuAction.action != null && menuAction.action.WasPressedThisFrame())
        {
            ToggleMenu();
        }
        
        if (menuActive && menuCanvas != null)
        {
            if (Vector3.Distance(menuCanvas.transform.position, playerTransform.position) > 3.0f)
            {
                menuActive = false;
                menuCanvas.SetActive(false);
                lineVisual.enabled = false;
            }
        }


        if (cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    void activateMagnetInput()
    {

        if (playerDoorCheck != null && playerDoorCheck.IsPlayerInsideDoor)
        {
            isMagnetActivated = false;
            wasMagnetActivated = false;
            return;
        }



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
            bool canActivate = false;
            // Check if magnet button is pressed
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastDistance))
            {
                if (lastHoveredObject != null && lastHoveredObject.canActivateMagnet(playerTransform, hit.normal, isLeftHand))
                {
                    Vector3Int dir = getActivationDirection(hit.normal);
                    if (isRightHand) dir = -dir; // Pull for left hand


                    Vector3Int targetCell = lastHoveredObject.currentCell + dir; // Target cell to move to

                    // Play audio
                    if (handAudio != null && activateClip != null)
                    {
                        handAudio.PlayOneShot(activateClip);
                    }
                    lastHoveredObject.MoveToCell(targetCell);
                    cooldownTimer = magnetCooldown; // Reset cooldown
                    canActivate = true;
                }
            }

            if (!canActivate)
            {
                // Play invalid audio
                if (handAudio != null && highlightInvalidClip != null)
                {
                    handAudio.PlayOneShot(invalidActivateClip);
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
                if (lastHoveredObject != null && lastHoveredObject.canActivateClimb(playerTransform, hit.normal))
                {
                    Vector3Int dir = getActivationDirection(hit.normal);

                    // Max distance check
                    Vector3 faceCenter = lastHoveredObject.transform.position + (Vector3)dir * (lastHoveredObject.CellSize * 0.5f);

                    Vector3 playerFeetPosition = lastHoveredObject.GetPlayerFeetPosition();
                    float distanceToFace = Vector3.Distance(playerFeetPosition, faceCenter);

                    if (distanceToFace > maxClimbDistance)
                    {
                        return;
                    }


                    if (lastHoveredObject.IsFaceClimbable(dir))
                    {
                        // Climb face

                        StartCoroutine(lastHoveredObject.ClimbFace());
                    }
                }
            }

        }
    }



    void updateHighlight()
    {
        if (playerDoorCheck != null && playerDoorCheck.IsPlayerInsideDoor)
        {
            // Clear highlight if inside door
            if (lastHoveredObject != null)
            {
                lastHoveredObject.ClearHighlight();
                lastHoveredObject = null;
            }
            if (lineVisual != null)
                lineVisual.enabled = false;
            return;
        }

        RaycastHit hit;

        if (menuActive)
        {
            // Keep line visual on for menu
            if (lineVisual != null)
                lineVisual.enabled = true;
            return;
        }

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

                    // Play audio 
                    if (handAudio != null)
                    {
                        if (canActivate)
                        {
                            handAudio.PlayOneShot(highlightValidClip);
                        }
                        else
                        {
                            handAudio.PlayOneShot(highlightInvalidClip);
                        }
                    }
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

    // Player Menu
    public void ToggleMenu()
    {
        menuActive = !menuActive;

        if (lineVisual != null)
            lineVisual.enabled = menuActive;

        // Show/hide menu canvas
        if (menuActive)
        {
            if (menuCanvas != null)
            {
                menuCanvas.SetActive(true);
                // Position menu canvas in front of player
                menuCanvas.transform.position = playerTransform.position + playerTransform.forward * 1.5f;
                menuCanvas.transform.rotation = Quaternion.LookRotation(playerTransform.forward, Vector3.up);

            }
        }
        else
        {
            if (menuCanvas != null)
            {
                menuCanvas.SetActive(false);

                lineVisual.enabled = false;
            }
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
