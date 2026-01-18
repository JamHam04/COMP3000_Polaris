using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CubeType
{
    Red,
    Blue,
    Purple
}



public class GridObject : MonoBehaviour
{
    
    
    public Vector3Int currentCell;
    private GridController gridController;

    public GameObject faceHighlightPrefab;
    private GameObject activeHighlight;
    public Vector3 highlightedFace;

    public float CellSize => gridController.cellSize;

    private Coroutine moveCoroutine; // Cube movement
    private float cubeMoveSpeed = 0.5f; // Speed of cube movement

    public HashSet<Vector3Int> disabledFaces = new HashSet<Vector3Int>(); // Disabled faces based on occupied adjacent cells

    Transform playerTransform;
    private bool isMoving = false;

    private Renderer cubeRenderer;
    private Vector3 lastPosition;
    CharacterController playerController;

    // Cube type

    public CubeType cubeType;

    // Start is called before the first frame update
    void Start()
    {
        // Get GridController script
        gridController = FindObjectOfType<GridController>();

        // Get Renderer for frame
        cubeRenderer = GetComponent<Renderer>();

        // Get player transform
        playerTransform = Camera.main.transform;

        // Player character controller
        playerController = playerTransform.GetComponentInParent<CharacterController>();

        // Initialize current cell
        currentCell = gridController.worldToCell(transform.position);
        MoveToCell(currentCell);

        lastPosition = transform.position;

    }

    // Update is called once per frame
    void Update()
    {


    }

    public void DisableFace(Vector3Int faceNormal)
    {
        disabledFaces.Add(faceNormal);
    }

    public void EnableFace(Vector3Int faceNormal)
    {
        disabledFaces.Remove(faceNormal);
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, Vector3Int newCell, float cubeMoveSpeed)
    {
        Vector3 startPosition = transform.position;
        Vector3Int startCell = currentCell;
        float elapsedTime = 0f;

        // Disable collider during movement (Prevent falling through floor)
        // Change to different method later?
        Collider cubeCollider = GetComponent<Collider>();
        //cubeCollider.enabled = false;

        lastPosition = transform.position;

        while (elapsedTime < cubeMoveSpeed)
        {
            // Get new position
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / cubeMoveSpeed);
            Vector3 movementDelta = newPosition - lastPosition;

            // Move cube
            transform.position = newPosition;

            // Check if player is on top of cube
            float playerY = playerTransform.position.y;
            float cubeY = transform.position.y;

            if ((playerY - cubeY) > 0)
            {
                // Player is above cube, move player up with cube
                playerController.Move(movementDelta);
                
            }



            // Check distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // If hitting player
            if (distanceToPlayer < gridController.cellSize * 0.7f) 
            {
                //cubeCollider.enabled = true; // Re-enable collider
                yield return StartCoroutine(BounceBack(startPosition, startCell, newCell));
                yield break;
            }
            lastPosition = transform.position;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        // Re-enable collider after movement
        //cubeCollider.enabled = true;

        // Exit current cell
        gridController.ExitCell(currentCell);
        // Update current cell
        currentCell = newCell;
        // Enter new cell
        gridController.EnterCell(newCell, this);
        // Unreserve new cell
        gridController.UnreserveCell(newCell);
        isMoving = false;
        UpdateDisabledFaces(); // Update disabled faces after moving


    }

    private IEnumerator BounceBack(Vector3 startPosition, Vector3Int startCell, Vector3Int newCell)
    {
        Vector3 currentPosition = transform.position;
        float elapsedTime = 0f;

        // Move back to start position
        while (elapsedTime < cubeMoveSpeed)
        {
            transform.position = Vector3.Lerp(currentPosition, startPosition, elapsedTime / cubeMoveSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition;

        gridController.UnreserveCell(newCell);
        isMoving = false;
        currentCell = startCell;
    }

    public void MoveToCell(Vector3Int newCell)
    {

        if (isMoving)
            return;

        if (!gridController.IsCellOccupied(newCell) && gridController.IsInGrid(newCell) && !gridController.IsCellDisabled(newCell) && !gridController.IsCellReserved (newCell))
        {
            gridController.ReserveCell(newCell, this); // Reserve new cell

            isMoving = true;

            // Smooth movement to new cell
            moveCoroutine = StartCoroutine(MoveToPosition(gridController.CellToWorld(newCell), newCell, cubeMoveSpeed));
        }
        else
        {
            Debug.Log("Cell occupied");
        }
    }


    public void HighlightFace(RaycastHit hit, Vector3 normal, bool canActivate)
    {
        Vector3Int snappedFaceNormal = SnapNormal(normal);
        Vector3Int pushCell = currentCell - snappedFaceNormal;
        Vector3Int pullCell = currentCell + snappedFaceNormal;

        bool canPull = gridController.IsInGrid(pullCell) && !gridController.IsCellOccupied(pullCell) && !gridController.IsCellDisabled(pullCell) && !gridController.IsCellReserved(pullCell);
        bool canPush = gridController.IsInGrid(pushCell) && !gridController.IsCellOccupied(pushCell) && !gridController.IsCellDisabled(pushCell) && !gridController.IsCellReserved(pushCell);

        // Instantiate new highlight
        if (activeHighlight == null)
        {
            activeHighlight = Instantiate(faceHighlightPrefab, transform); // parent to grid object
            activeHighlight.transform.localPosition = Vector3.zero;
            activeHighlight.transform.localScale = Vector3.one;

        }

        // Invaid face if cannot activate
        bool invalidFace = !canActivate;  

        // Handle highlight for cube types
        switch (cubeType)
        {
            case CubeType.Blue:
                // Enable face only if there is a cell free to push into
                if (!canPull) 
                {
                    invalidFace = true;
                }


                break;
            case CubeType.Red:
                // Enable face only if there is a cell free to pull into
                if (!canPush)
                {
                    invalidFace = true;
                }
                break;
            case CubeType.Purple:
                // Enable face only if there is a cell free to push or pull into

                if (!canPull && !canPush)
                {
                    invalidFace = true;
                }
                break;
        }

        Renderer highlightRenderer = activeHighlight.GetComponentInChildren<Renderer>();
        if (invalidFace)
        {
            // Set highlight color to red
            highlightRenderer.material.color = Color.gray;
        }
        else
        {
            // Set highlight color to green
            highlightRenderer.material.color = Color.cyan;
        }



        highlightedFace = snappedFaceNormal;

        activeHighlight.transform.rotation = Quaternion.LookRotation(snappedFaceNormal ); // face direction
    }


    public void ClearHighlight()
    {
        if (activeHighlight != null)
        {
            Destroy(activeHighlight);
            activeHighlight = null;
            highlightedFace = Vector3.zero;

        }
    }

    // Snap to axis 
    public static Vector3Int SnapNormal(Vector3 normal)
    {
        // Normalize
        normal.Normalize();

        // Get absolute values of each axis
        float absX = Mathf.Abs(normal.x);
        float absY = Mathf.Abs(normal.y);
        float absZ = Mathf.Abs(normal.z);

        // Pick the axis with largest absolute value
        if (absX >= absY && absX >= absZ)
            return new Vector3Int(normal.x > 0 ? 1 : -1, 0, 0);
        else if (absY >= absX && absY >= absZ)
            return new Vector3Int(0, normal.y > 0 ? 1 : -1, 0);
        else
            return new Vector3Int(0, 0, normal.z > 0 ? 1 : -1);
    }

    public bool canActivateMagnet(Transform playerHead, Vector3 faceNormal, bool isLeftHand)
    {
        Vector3Int snappedFaceNormal = SnapNormal(faceNormal); // Use snapped normal for face direction

        // Check adjacent cell in face normal direction
        Vector3Int pushCell = currentCell - snappedFaceNormal;
        Vector3Int pullCell = currentCell + snappedFaceNormal;

        bool canPull = gridController.IsInGrid(pullCell) && !gridController.IsCellOccupied(pullCell) && !gridController.IsCellDisabled(pullCell) && !gridController.IsCellReserved(pullCell);
        bool canPush = gridController.IsInGrid(pushCell) && !gridController.IsCellOccupied(pushCell) && !gridController.IsCellDisabled(pushCell) && !gridController.IsCellReserved(pushCell);

        // Check face for each cube type
        switch (cubeType)
        {
            case CubeType.Blue:
                // Can only push if adjacent cell is free
                if (!canPull)
                {
                    return false;
                }

                break;
            case CubeType.Red:
                // Can only pull if adjacent cell is occupied
                if (!canPush)
                {
                    return false;
                }
                break;
            case CubeType.Purple:
                if (isLeftHand)
                {
                    // Left hand can only pull if adjacent cell is free
                    if (!canPull)
                    {
                        return false;
                    }
                }
                else
                {
                    // Right hand can only push if adjacent cell is free
                    if (!canPush)
                    {
                        return false;
                    }
                }
                break;
        }

        // Check is player is in front of gridobject face
        Vector3 faceCenter = transform.position + (Vector3)snappedFaceNormal * (gridController.cellSize / 2f);// Cube face center

        //float distanceToPlayer = Vector3.Distance(playerPos, faceCenter);

        Vector3 toPlayer = (playerHead.position - faceCenter).normalized;
        float toPlayerDot = Vector3.Dot(snappedFaceNormal, toPlayer);

        // Check if player is in front of face
        if (toPlayerDot < 0.2f)
        {
            return false; // Player is behind the face
        }

        
        Vector3 toFace = (faceCenter - playerHead.position).normalized;
        float toFaceDot = Vector3.Dot(playerHead.forward, toFace);

        // Check if player is looking at face
        if (toFaceDot < 0.3f)
        {
            return false; // Player is not looking at the face
        }


        // Distance checks
        float maxDistance = gridController.cellSize * 2f;
        if (Vector3.Distance(playerHead.position, faceCenter) > maxDistance)
            return false;

        // Vertical angle check:
   


        return true;

    }

    public void UpdateDisabledFaces()
    {
        disabledFaces.Clear(); // Reset disabled faces on move

        // Directions for cube faces
        Vector3Int[] directions = {
        Vector3Int.right, Vector3Int.left,
        Vector3Int.up, Vector3Int.down,
        Vector3Int.forward, Vector3Int.back
    };

        // Check each adjacent cell
        foreach (var dir in directions)
        {
            Vector3Int adjacentCell = currentCell + dir;
            // Check if cell is occupied 
            if (gridController.IsCellOccupied(adjacentCell))
            {
                DisableFace(dir); // Disable face 
            }
            // Leave face enabled
            else
            {
                EnableFace(dir);
            }
        }
    }

    // Hand types 
    public bool handType(bool isLeftHand)
    {
        switch (cubeType)
        {
            case CubeType.Blue:
                return isLeftHand; // Blue cube for left hand
            case CubeType.Red:
                return !isLeftHand; // Red cube for right hand
            case CubeType.Purple:
                return true; // Purple cube for both hands
            default:
                return false;
        }
    }

    public void SetEmission(bool enableGlow)
    {
        if (enableGlow)
        {
            cubeRenderer.material.EnableKeyword("_EMISSION"); // Enable emission
        }
        else
        {
            cubeRenderer.material.DisableKeyword("_EMISSION"); // Diable emission
        }
    }


}
