using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class GridObject : MonoBehaviour
{
    
    
    public Vector3Int currentCell;
    private GridController gridController;

    public GameObject faceHighlightPrefab;
    private GameObject activeHighlight;

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

    public void MoveToCell(Vector3Int newCell)
    {
        if (!gridController.IsCellOccupied(newCell) && gridController.IsInGrid(newCell))
        {
            // Exit current cell
            gridController.ExitCell(currentCell);

            // Update current cell
            currentCell = newCell;

            // Enter new cell
            gridController.EnterCell(newCell, this);


            // Move object to new cell
            transform.position = gridController.CellToWorld(newCell);

            
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
        }
        

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

        }
    }


    public bool canActivateMagnet(Transform player)
    {
        // Check player position 
        Vector3 playerPos = player.position;

        // Check is player is in front of gridobject face
        Vector3 toPlayer = (playerPos - transform.position).normalized;
        float facingDirection = Vector3.Dot(transform.forward, toPlayer); // Positive if facing player (ONLY CHECKS FRONT FACE)
        // check all faces


        //Debug.Log("Facing direction: " + facingDirection);
        if (facingDirection < 0.2f)
        {
            return false; // Player is behind the face
        }

        return true;
    }

}
