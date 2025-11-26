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

    // Cube type

    public CubeType cubeType;

    // Start is called before the first frame update
    void Start()
    {
        // Get GridController script
        gridController = FindObjectOfType<GridController>(); 

        // Initialize current cell
        currentCell = gridController.worldToCell(transform.position);
        MoveToCell(currentCell);



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

    private IEnumerator MoveToPosition(Vector3 targetPosition, float cubeMoveSpeed)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < cubeMoveSpeed)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / cubeMoveSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Grid position
    }

    public void MoveToCell(Vector3Int newCell)
    {
        if (!gridController.IsCellOccupied(newCell) && gridController.IsInGrid(newCell) && !gridController.IsCellDisabled(newCell))
        {
            // Exit current cell
            gridController.ExitCell(currentCell);

            // Update current cell
            currentCell = newCell;

            // Enter new cell
            gridController.EnterCell(newCell, this);

            // Stop previous movement
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            // Smooth movement to new cell
            moveCoroutine = StartCoroutine(MoveToPosition(gridController.CellToWorld(newCell), cubeMoveSpeed));
            UpdateDisabledFaces(); // Update disabled faces after moving
        }
        else
        {
            Debug.Log("Cell occupied");
        }
    }


    public void HighlightFace(RaycastHit hit)
    {

        // Instantiate new highlight
        if (activeHighlight == null)
        {
            activeHighlight = Instantiate(faceHighlightPrefab); // use prefab for highlight
            activeHighlight.transform.SetParent(transform); // parent to grid object
        }

        highlightedFace = hit.normal;


        // Position and direction based on raycast hit
        activeHighlight.transform.position = transform.position + hit.normal * (gridController.cellSize / 2f); // position at face (spawn at centre of face)
        activeHighlight.transform.rotation = Quaternion.LookRotation(-hit.normal); // face direction

        // Apply position offset
        activeHighlight.transform.position += hit.normal * 0.01f;
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


    public bool canActivateMagnet(Transform playerHead, Vector3 faceNormal)
    {
        Vector3Int faceNormalInt = new Vector3Int(
            Mathf.RoundToInt(faceNormal.x),
            Mathf.RoundToInt(faceNormal.y),
            Mathf.RoundToInt(faceNormal.z)
        );

        if (disabledFaces.Contains(faceNormalInt))
            return false;

        // Check is player is in front of gridobject face
        Vector3 faceCenter = transform.position + faceNormal * (gridController.cellSize / 2f); // Cube face center

        //float distanceToPlayer = Vector3.Distance(playerPos, faceCenter);

        Vector3 toPlayer = (playerHead.position - faceCenter).normalized;
        float toPlayerDot = Vector3.Dot(faceNormal, toPlayer);

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

        // Controller checks??

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

    


}
