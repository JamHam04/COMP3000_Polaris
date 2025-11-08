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
