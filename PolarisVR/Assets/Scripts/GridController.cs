using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridController : MonoBehaviour
{
    // Grid dimensions
    public int gridX = 10; // Width of the grid in cells
    public int gridY = 10; // Height of the grid in cells
    public int gridZ = 10;  // Depth of the grid in cells
    public float cellSize = 1.0f; // Size of each cell in the grid
    public Vector3 gridCoordinates = Vector3.zero; // Where grid starts in world

    // Grid cell occupancy
    private Dictionary<Vector3Int, GridObject> occupiedCells = new Dictionary<Vector3Int, GridObject>(); // Dictionary of occupied cells
    private Dictionary<Vector3Int, GridObject> reservedCells = new Dictionary<Vector3Int, GridObject>(); // Dictionary of reserved cells
    public List<DisabledSection> disabledRegions = new List<DisabledSection>();

    public GameObject floorCellPrefab;

    void Start()
    {

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                Vector3Int cell = new Vector3Int(x, 0, z); 

                if (IsCellDisabled(cell)) continue; // skip disabled cells

                // Spawn floor cell
                GameObject floorCell = Instantiate(floorCellPrefab, transform);
                GameObject roofCell = Instantiate(floorCellPrefab, transform);

                Vector3 pos = CellToWorld(cell);
                pos.y = gridCoordinates.y + 0.0001f;
                floorCell.transform.position = pos; // set position

                pos.y = gridCoordinates.y + gridY;
                roofCell.transform.Rotate(180f, 0f, 0f);
                roofCell.transform.position = pos; // set position
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Converts world position to cell coordinates
    public Vector3Int worldToCell(Vector3 worldPos)
    {
        // Adjust for grid start offset
        worldPos -= gridCoordinates;
        // Calculate cell coordinates 
        int x = Mathf.FloorToInt(worldPos.x / cellSize); 
        int y = Mathf.FloorToInt(worldPos.y / cellSize);
        int z = Mathf.FloorToInt(worldPos.z / cellSize);
        return new Vector3Int(x, y, z);

    }

    // Converts cell coordinates to world position
    public Vector3 CellToWorld(Vector3Int cellCoords)
    {
        // Calculate world position
        float x = cellCoords.x * cellSize + cellSize / 2;
        float y = cellCoords.y * cellSize + cellSize / 2;
        float z = cellCoords.z * cellSize + cellSize / 2;
        return new Vector3(x, y, z) + gridCoordinates; // Add grid start offset
    }

    // Draw grid for visualization
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // Grid color

        // Draw grid lines
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                for (int z = 0; z < gridZ; z++)
                {
                    // Skip disabled cells
                    if (IsCellDisabled(new Vector3Int(x, y, z))) continue; // Don't draw disabled cells
                    Vector3 pos = CellToWorld(new Vector3Int(x, y, z));
                    Gizmos.DrawWireCube(pos, Vector3.one * cellSize);
                }

            }
        }
    }

    // If cell is occupied
    public bool IsCellOccupied(Vector3Int cellCoords)
    {
        // Check if the cell coordinates exist in the occupiedCells dictionary
        if (occupiedCells.ContainsKey(cellCoords))
        {
            return true;
        }
        return false;
    }

    public bool IsCellReserved(Vector3Int cell)
    {
        if (reservedCells.ContainsKey(cell)) {
            return true;
        }
        return false;
    }

    // If cell is disabled
    public bool IsCellDisabled(Vector3Int cell)
    {
        // Loop through disabled regions
        foreach (var section in disabledRegions)
        {
            // Check if cell is within disabled section
            if (cell.x >= section.startCell.x && cell.x < section.startCell.x + section.size.x &&
                cell.y >= section.startCell.y && cell.y < section.startCell.y + section.size.y &&
                cell.z >= section.startCell.z && cell.z < section.startCell.z + section.size.z)
            {
                return true; // Cell is disabled
            }
        }
        return false; // Cell is not disabled
    }

    // Check grid bounds
    public bool IsInGrid(Vector3Int cellCoords)
    {
        if (cellCoords.x >= 0 && cellCoords.x < gridX &&
            cellCoords.y >= 0 && cellCoords.y < gridY &&
            cellCoords.z >= 0 && cellCoords.z < gridZ)
        {
            return true;
        }
        return false;
    }

    // Object enters cell
    public void EnterCell(Vector3Int cellCoords, GridObject obj)
    {
        
        // Add the object and position to dictionary
        if (!occupiedCells.ContainsKey(cellCoords))
        {
            occupiedCells.Add(cellCoords, obj);
        }
    }

    // Object leaves cell
    public void ExitCell(Vector3Int cellCoords)
    {
        // Remove from dictionary
        if (occupiedCells.ContainsKey(cellCoords))
        {
            occupiedCells.Remove(cellCoords);
        }
    }

    // Reserve cell
    public void ReserveCell(Vector3Int cellCoords, GridObject obj)
    {
        if (!reservedCells.ContainsKey(cellCoords))
        {
            reservedCells.Add(cellCoords, obj);
        }
    }

    // Unreserve cell
    public void UnreserveCell(Vector3Int cellCoords)
    {
        if (reservedCells.ContainsKey(cellCoords))
        {
            reservedCells.Remove(cellCoords);
        }
    }

    public bool playerInCell(Vector3Int cellCoords, Transform playerTransform)
    {
        Vector3Int playerCell = worldToCell(playerTransform.position);
        return playerCell == cellCoords;
    }

    public GridObject GetCubeInCell(Vector3Int cellCoords)
    {
        if (occupiedCells.ContainsKey(cellCoords))
        {
            return occupiedCells[cellCoords];
        }
        return null;
    }

}

