using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridController : MonoBehaviour
{
    // Grid dimensions
    public int gridX = 10; // Width of the grid in cells
    public int gridY = 10; // Height of the grid in cells
    public int gridZ = 10;  // Depth of the grid in cells
    public float cellSize = 2.0f; // Size of each cell in the grid
    public Vector3 gridCoordinates = Vector3.zero; // Where grid starts in world

    // Grid cell occupancy
    private Dictionary<Vector3Int, GridObject> occupiedCells = new Dictionary<Vector3Int, GridObject>(); // Dictionary of occupied cells
    private Dictionary<Vector3Int, GridObject> reservedCells = new Dictionary<Vector3Int, GridObject>(); // Dictionary of reserved cells
    public List<DisabledSection> disabledRegions = new List<DisabledSection>();

    public GameObject floorCellPrefab;

    // pillars
    public GameObject cornerPillarPrefab;
    public Material pillarMaterial;

    // Grid outline
    public GameObject outlinePrefab;
    public bool showGrid = true;
    public GameObject borderTile;



    private const float zOffset = 0.001f; // Offset to prevent z-clipping

    // Magnet rules (player positioning)
    public int maxHorizontalDistance = 2; // Max horizontal cells from player to cellcube
    public int maxVerticalDistance = 1; // Max vertical cells from player to cellcube




    void Start()
    {
        CreateCornerPillars();
        CreateTopOutline();
        CreateBottomOutline();

        CreateGridWalls();


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

    // Corner to world position
    public Vector3 CornerToWorld(Vector3Int cornerCoords)
    {
        // Calculate world position
        float x = cornerCoords.x * cellSize;
        float y = cornerCoords.y * cellSize;
        float z = cornerCoords.z * cellSize;
        return new Vector3(x, y, z) + gridCoordinates; // Add grid start offset
    }

    // Draw grid for visualization
    private void OnDrawGizmos()
    {
        if (!showGrid) return;

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

    // If cell is enabled
    bool IsCellEnabled(Vector3Int cell)
    {
        return IsInGrid(cell) && !IsCellDisabled(cell);
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

    // Check if player is in 3x3 cells in front of face normal
    public bool IsPlayerInFrontCells(Vector3Int cellCoords, Vector3Int faceNormal, Vector3 playerPosition)
    {
        Vector3Int playerCell = worldToCell(playerPosition); // Player position relative to grid


        // Horizontal faces
        if (faceNormal.y == 0)
        {
            // Check vertical distance 
            int distanceY = Mathf.Abs(playerCell.y - cellCoords.y);
            if (distanceY > 1) return false;

            // Check horizontal ditance in front of face
            int distanceX = Mathf.Abs(playerCell.x - cellCoords.x);
            int distanceZ = Mathf.Abs(playerCell.z - cellCoords.z);

            // Check 3x3 cells in front of face normal
            if (distanceX <= maxHorizontalDistance && distanceZ <= maxHorizontalDistance) return true;

            return false;
        }
        // Vertical faces
        else
        {
            // Check vertical distance 
            int distanceY = Mathf.Abs(playerCell.y - cellCoords.y);
            if (distanceY > maxVerticalDistance) return false;

            // Checkhorizontal ditance in front of face
            int distanceX = Mathf.Abs(playerCell.x - cellCoords.x);
            int distanceZ = Mathf.Abs(playerCell.z - cellCoords.z);

            // Check 3x3 cells in front of face normal
            if (distanceX <= 1 && distanceZ <= 1) return true; // X and Z around the 0,0 centre.

            return false;

        }
    }



    public GridObject GetCubeInCell(Vector3Int cellCoords)
    {
        if (occupiedCells.ContainsKey(cellCoords))
        {
            return occupiedCells[cellCoords];
        }
        return null;
    }

    // Create pillars on grid corners
    void CreateCornerPillars()
    {
        // Create list of croners
        HashSet<Vector3Int> gridCorners = new HashSet<Vector3Int>();

        // Check each cell in grid
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                for (int z = 0; z < gridZ; z++)
                {
                    // Skip disabled cells
                    if (IsCellDisabled(new Vector3Int(x, y, z))) continue;

                    // Check cells around corner, add corner if edge corner
                    AddCorner(gridCorners, new Vector3Int(x, y, z));
                    AddCorner(gridCorners, new Vector3Int(x + 1, y, z));
                    AddCorner(gridCorners, new Vector3Int(x, y, z + 1));
                    AddCorner(gridCorners, new Vector3Int(x + 1, y, z + 1));
                }
            }
        }

        // Create pillar at each corner
        foreach (var corner in gridCorners)
        {
            // Chgeck top and bottom y levels for pillar height
            int topYLevel = Mathf.Max(
            GetTopYLevel(corner),
            GetTopYLevel(corner + new Vector3Int(-1, 0, 0)),
            GetTopYLevel(corner + new Vector3Int(0, 0, -1)),
            GetTopYLevel(corner + new Vector3Int(-1, 0, -1))
            );
            int bottomYLevel = Mathf.Min(
            GetBottomYLevel(corner),
            GetBottomYLevel(corner + new Vector3Int(-1, 0, 0)),
            GetBottomYLevel(corner + new Vector3Int(0, 0, -1)),
            GetBottomYLevel(corner + new Vector3Int(-1, 0, -1))
            );

            float pillarHeight = (topYLevel - bottomYLevel + 1) * cellSize; // Calculate pillar height to grid size

            // Instantiate pillar
            GameObject pillar = Instantiate(cornerPillarPrefab, transform);

            // Position pillar at corner
            Vector3 pillarPos = CornerToWorld(corner);
            pillarPos.y = gridCoordinates.y + (bottomYLevel * cellSize) + (pillarHeight / 2); // Set to bottom y level (+ half height)
            pillar.transform.position = pillarPos;

            // Scale pillar to grid height
            Vector3 pillarScale = pillar.transform.localScale;
            pillarScale.y = (topYLevel - bottomYLevel + 1);
            pillar.transform.localScale = pillarScale; 

            // Set pillar material
            pillar.GetComponent<Renderer>().material = pillarMaterial;
            
        }
    }

    void AddCorner(HashSet<Vector3Int> gridCorners, Vector3Int corner)
    {
        // Check the 4 cells around this corner point
        bool bottomLeft = IsCellEnabled(corner + new Vector3Int(-1, 0, -1));
        bool bottomRight = IsCellEnabled(corner + new Vector3Int(0, 0, -1));
        bool topLeft = IsCellEnabled(corner + new Vector3Int(-1, 0, 0));
        bool topRight = IsCellEnabled(corner);

        // Count enabled cells
        int enabledCellCount = 0;
        if (bottomLeft) enabledCellCount++;
        if (bottomRight) enabledCellCount++;
        if (topLeft) enabledCellCount++;
        if (topRight) enabledCellCount++;

        // If 1 or 3 of the surrounding cells are enabled, this is an edge corner (1 cell are end outer corner, 3 cells are inner corner)
        if (enabledCellCount == 1 || enabledCellCount == 3)
        {
            gridCorners.Add(corner); // Add corner to set
        }
    }


    // Create outline at top of grid
    void CreateTopOutline()
    {
        float outlineY = gridCoordinates.y + gridY * cellSize; // Top of grid

        // Check each cell at the top layer
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++) 
            {
                for (int z = 0; z < gridZ; z++)
                {

                    // Skip disabled cells
                    if (IsCellDisabled(new Vector3Int(x, y, z))) continue;

                    if (IsCellEnabled(new Vector3Int(x, y + 1, z))) continue;

                    // Check cells around, if disabled then it is an edege
                    bool leftEdge = !IsCellEnabled(new Vector3Int(x - 1, y, z)) || GetTopYLevel(new Vector3Int(x - 1, y, z)) != y;
                    bool rightEdge = !IsCellEnabled(new Vector3Int(x + 1, y, z)) || GetTopYLevel(new Vector3Int(x + 1, y, z)) != y;
                    bool frontEdge = !IsCellEnabled(new Vector3Int(x, y, z - 1)) || GetTopYLevel(new Vector3Int(x, y, z - 1)) != y;
                    bool backEdge = !IsCellEnabled(new Vector3Int(x, y, z + 1)) || GetTopYLevel(new Vector3Int(x, y, z + 1)) != y;

                    // Get cell center position
                    Vector3 cellCenter = CellToWorld(new Vector3Int(x, y, z)); // World pos of cell center
                    cellCenter.y = (gridCoordinates.y + (y + 1) * cellSize) - 0.026f; // Top - half size of outline prefav (adding 0.001f to prevent z-fighting)

                    // Create outline segment for each edge
                    if (leftEdge) CreateEdge(cellCenter + Vector3.left * cellSize / 2, Vector3.forward);
                    if (rightEdge) CreateEdge(cellCenter + Vector3.right * cellSize / 2, Vector3.forward);
                    if (frontEdge) CreateEdge(cellCenter + Vector3.back * cellSize / 2, Vector3.right);
                    if (backEdge) CreateEdge(cellCenter + Vector3.forward * cellSize / 2, Vector3.right);

                }
            }
        }
    }

    void CreateBottomOutline()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, z);

                    // Skip disabled cells
                    if (!IsCellEnabled(cell)) continue; 

                    
                    if (y > 0 && IsCellEnabled(new Vector3Int(x, y - 1, z))) continue;

                    // Check cells around, if disabled then it is an edege
                    bool leftEdge = !IsCellEnabled(new Vector3Int(x - 1, y, z)) || GetBottomYLevel(new Vector3Int(x - 1, y, z)) != y;
                    bool rightEdge = !IsCellEnabled(new Vector3Int(x + 1, y, z)) || GetBottomYLevel(new Vector3Int(x + 1, y, z)) != y;
                    bool frontEdge = !IsCellEnabled(new Vector3Int(x, y, z - 1)) || GetBottomYLevel(new Vector3Int(x, y, z - 1)) != y;
                    bool backEdge = !IsCellEnabled(new Vector3Int(x, y, z + 1)) || GetBottomYLevel(new Vector3Int(x, y, z + 1)) != y;

                    // Get cell center position
                    Vector3 cellCenter = CellToWorld(cell); // World pos of cell center
                    cellCenter.y = gridCoordinates.y + y * cellSize + 0.026f;

                    // Create outline segment for each edge
                    if (leftEdge) CreateEdge(cellCenter + Vector3.left * cellSize / 2, Vector3.forward);
                    if (rightEdge) CreateEdge(cellCenter + Vector3.right * cellSize / 2, Vector3.forward);
                    if (frontEdge) CreateEdge(cellCenter + Vector3.back * cellSize / 2, Vector3.right);
                    if (backEdge) CreateEdge(cellCenter + Vector3.forward * cellSize / 2, Vector3.right);
                }
            }
        }
    }

    int GetTopYLevel(Vector3Int cellCoords) 
    {
        for (int y = gridY - 1; y >= 0; y--)
        {
            if (IsCellEnabled(new Vector3Int(cellCoords.x, y, cellCoords.z)))
            {
                return y;
            }
        }
        return -1; // No enabled cells
    }

    int GetBottomYLevel(Vector3Int cellCoords)
    {
        for (int y = 0; y < gridY; y++)
        {
            if (IsCellEnabled(new Vector3Int(cellCoords.x, y, cellCoords.z)))
            {
                return y;
            }
        }
        return gridY; // No enabled cells
    }


    void CreateEdge(Vector3 position, Vector3 direction)
    {
        GameObject outlineEdge = Instantiate(outlinePrefab, transform);
        outlineEdge.transform.position = position;

        Vector3 stretchScale = outlinePrefab.transform.localScale;

        // Set length to size of cell
        if (direction == Vector3.forward || direction == Vector3.back)
        {
            stretchScale.z = cellSize; // stretch along z (front and back)
        }
        else // right or left
        {
            stretchScale.x = cellSize; // stretch along x (right and left)
        }
        outlineEdge.GetComponent<Renderer>().material = pillarMaterial; // placeholder
        outlineEdge.transform.localScale = stretchScale;

        // Draw edges around bottom as well??
    }

    // Add border around grid
    void CreateGridWalls()
    {
        GameObject hologramParent = new GameObject("GridWalls");
        hologramParent.transform.parent = transform;

        for (int x = 0; x < gridX; x++)
            for (int y = 0; y < gridY; y++)
                for (int z = 0; z < gridZ; z++)
                {
                    Vector3Int cell = new Vector3Int(x, y, z);
                    if (!IsCellEnabled(cell)) continue;

                    // Check cells around for walls
                    if (!IsCellEnabled(cell + new Vector3Int(-1, 0, 0))) AddGridWall(cell, Vector3.left, hologramParent.transform);
                    if (!IsCellEnabled(cell + new Vector3Int(1, 0, 0))) AddGridWall(cell, Vector3.right, hologramParent.transform);
                    if (!IsCellEnabled(cell + new Vector3Int(0, 0, -1))) AddGridWall(cell, Vector3.back, hologramParent.transform);
                    if (!IsCellEnabled(cell + new Vector3Int(0, 0, 1))) AddGridWall(cell, Vector3.forward, hologramParent.transform);

                    // Roof cells 
                    if (!IsCellEnabled(cell + new Vector3Int(0, 1, 0))) AddGridRoof(cell, hologramParent.transform);

                    // Floor cells
                    if (!IsCellEnabled(cell + new Vector3Int(0, -1, 0))) AddGridFloor(cell, hologramParent.transform);
                }
    }

    // Create wall quads
    void AddGridWall(Vector3Int cell, Vector3 direction, Transform parent)
    {
        // Create and child quad
        GameObject borderQuad = Instantiate(borderTile, parent);


        float height = cellSize;
        borderQuad.transform.localScale = new Vector3(cellSize, height, 1);

        // Move to edeg
        Vector3 quadPosition = CellToWorld(cell); // Center of cell
        quadPosition.y = gridCoordinates.y + (cell.y * cellSize) + height / 2; // Set y to middle of wall height
        quadPosition += direction * (cellSize / 2 - zOffset); // Move to grid edge (depending on direction)
        borderQuad.transform.position = quadPosition; // Set position

        // Rotate based on direction
        if (direction == Vector3.left)
            borderQuad.transform.rotation = Quaternion.Euler(0, 90, 0);
        else if (direction == Vector3.right)
            borderQuad.transform.rotation = Quaternion.Euler(0, -90, 0);
        else if (direction == Vector3.back)
            borderQuad.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (direction == Vector3.forward)
            borderQuad.transform.rotation = Quaternion.Euler(0, 180, 0);

    } 

    // Create roof quads
    void AddGridRoof(Vector3Int cell, Transform parent)
    {
        // Create and child quad
        GameObject borderQuad = Instantiate(borderTile, parent);

        borderQuad.transform.localScale = Vector3.one * cellSize;

        Vector3 quadPosition = CellToWorld(cell); // Center of cell

        quadPosition.y += cellSize / 2; // Move to top of cell
        quadPosition.y += zOffset; // Prevent z-fighting
        borderQuad.transform.position = quadPosition; // Set position 
        borderQuad.transform.rotation = Quaternion.Euler(90, 0, 0); // Rotate uopwards

    }

    void AddGridFloor(Vector3Int cell, Transform parent)
    {
        // Create and child quad
        GameObject borderQuad = Instantiate(borderTile, parent);

        borderQuad.transform.localScale = Vector3.one * cellSize;

        Vector3 quadPosition = CellToWorld(cell); // Center of cell

        quadPosition.y -= cellSize / 2; // Move to bottom of cell
        quadPosition.y -= zOffset; // Prevent z-fighting
        borderQuad.transform.position = quadPosition; // Set position 
        borderQuad.transform.rotation = Quaternion.Euler(-90, 0, 0); // Rotate downwards
    }



}

