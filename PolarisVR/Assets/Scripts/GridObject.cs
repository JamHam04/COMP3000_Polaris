using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class GridObject : MonoBehaviour
{
    
    public Vector3Int currentCell;
    private GridController gridController;

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
        // Movement test input (move to input system script later)

        Vector3Int targetCell = currentCell;

        if (Input.GetKeyDown(KeyCode.W))
        {
            targetCell += Vector3Int.forward;
            MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            targetCell += Vector3Int.back;
            MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            targetCell += Vector3Int.left;
            MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            targetCell += Vector3Int.right;
            MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            targetCell += Vector3Int.up;
            MoveToCell(targetCell);
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            targetCell += Vector3Int.down;
            MoveToCell(targetCell);
        }


        


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

}
