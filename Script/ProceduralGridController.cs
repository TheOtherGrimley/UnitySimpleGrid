using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGridController : MonoBehaviour
{
    public enum GridDirection { xy, xz, yz}
    [SerializeField] private GameObject CellPrefab;
    [SerializeField] private GameObject LinesPrefab;
    
    [Tooltip("The size of each individual cell")]
    [SerializeField] private float _cellSize = 0.1f;

    [Tooltip("The distance between each cell")]
    [SerializeField] private float _cellPadding = 0.1f;

    [SerializeField] private Vector2 GridSize;
    [SerializeField] private GridDirection _gridDirection;

    [SerializeField] private bool _initialiseOnPlay;

    private GameObject SpawnedLines;
    private Dictionary<Vector2, Cell> _cells = new Dictionary<Vector2, Cell>();

    public Dictionary<Vector2, Cell> Cells => _cells;

    public float CellSize => _cellSize;

    public float CellPadding => _cellPadding;

    // Start is called before the first frame update
    void Start()
    {
         InitialiseGrid();
    }

    [ContextMenu("Initialise Grid")]
    void InitialiseGrid()
    {
        ClearGrid();
        
        SpawnedLines = Instantiate(LinesPrefab, transform.position, Quaternion.identity, transform);
        SpawnedLines.transform.localScale = new Vector3(GridSize.x * ((_cellSize*10) + _cellPadding), 1f,
            GridSize.y * (_cellSize*10 + _cellPadding));
        
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                float w = _cellSize*10;
                float p = _cellPadding;
                float d = w + p;
                Vector3 offset = Vector3.zero;
                offset= new Vector3((x * d), 0f, (y * d));
                
                GameObject CellPlane = Instantiate(CellPrefab, this.transform.position,
                    Quaternion.identity, transform);
                Cell t_cell = CellPlane.GetComponent<Cell>();
                
                t_cell.CellPlane = CellPlane;
                t_cell.CellPlane.transform.localScale = new Vector3(_cellSize, 1f, _cellSize);
                t_cell.CellPlane.transform.localPosition = offset;
                t_cell.GridCoord = new Vector2(x, y);
                
                _cells[t_cell.GridCoord] = t_cell;
            }
        }

        switch (_gridDirection)
        {
            case GridDirection.xz:
                break;
            case GridDirection.xy:
                this.transform.Rotate(Vector3.right, -90);
                break;
            case GridDirection.yz:
                this.transform.Rotate(Vector3.forward, 90);
                this.transform.Rotate(Vector3.up, 90);
                break;
            
        }
    }

    public void RedrawGrid()
    {
        ClearGrid();

        InitialiseGrid();
    }

    [ContextMenu("Clear Grid")]
    private void ClearGrid()
    {
        this.transform.rotation = Quaternion.Euler(new Vector3(0,0,0));

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            #if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
            #else
            Destroy(transform.GetChild(i).gameObject);
            #endif
        }
    }

    public void ChangeCellSize(string NewCellSize)
    {
        _cellSize = float.Parse(NewCellSize);
        RedrawGrid();
    }
    
    public void ChangeCellSize(float NewCellSize)
    {
        _cellSize = NewCellSize;
        RedrawGrid();
    }
    
    public void ChangeLineWeight(string NewLineWeight)
    {
        _cellPadding = float.Parse(NewLineWeight);
        RedrawGrid();
    }
    
    public void ChangeLineWeight(float NewLineWeight)
    {
        _cellPadding = NewLineWeight;
        RedrawGrid();
    }

    private void OnDrawGizmosSelected()
    {
        // TODO: WORK THIS MATH OUT FOR OTHER 2 PLANES
        // Draw the grid outline when selected
        Gizmos.DrawLine(transform.position, transform.position + transform.right * ((GridSize.x-1) * (_cellSize*10 + _cellPadding)));
        Gizmos.DrawLine(transform.position + transform.right * ((GridSize.x-1) * (_cellSize*10 + _cellPadding)), (transform.position + transform.right * ((GridSize.x-1) * (_cellSize*10 + _cellPadding))) + transform.forward * ((GridSize.y-1) * (_cellSize*10 + _cellPadding)));
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * ((GridSize.y-1) * (_cellSize*10 + _cellPadding)));
        Gizmos.DrawLine(transform.position + transform.forward * ((GridSize.y-1) * (_cellSize*10 + _cellPadding)), transform.position + transform.forward * ((GridSize.y-1) * (_cellSize*10 + _cellPadding)) + transform.right * ((GridSize.x-1) * (_cellSize*10 + _cellPadding)));
    }
    
    /// <summary>
    /// Take a cell start and end point and will return a list of points along a line on the grid
    /// </summary>
    /// <param name="p0">initial point</param>
    /// <param name="p1">end point</param>
    /// <param name="points">list of points from start to finish along the line inc. p0 and p1</param>
    public void DrawLine(Vector2 p0, Vector2 p1, ref List<Vector2> points) {
        points.Clear();
        
        Vector2 p;
        float dx = p1.x - p0.x;
        float dy = p1.y - p0.y;
        float N = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        float divN = (N == 0)? 0.0f : 1.0f / N;
        float xstep = dx * divN;
        float ystep = dy * divN;
        float x = p0.x, y = p0.y;
        for (int step = 0; step <= N; step++, x += xstep, y += ystep) {
            p = new Vector2(Mathf.Round(x), Mathf.Round(y));
            points.Add(p);
        }
    }

    public void DrawCircleFromIntersect(Vector2 Centre, int radius, ref List<Vector2> points)
    {
        int r = radius;
        int SquaresRight = r ;
        int SquaresLeft = r ;
        int SquaresUp = r;
        int SquaresDown = r;

        // We count from 0 so drop 1 on right and up
        r -= 1;
        SquaresRight -= 1;
        SquaresUp -= 1;
        
        Vector2 OffsetCorrection = Vector2.zero;
            
        // Currently the best way to draw from intersect is to check 4 times in a circle
        for (int i = 0; i < 4; i++)
        {
            if (i == 0)
                OffsetCorrection = new Vector2(1, 1);
            if (i == 1)
                OffsetCorrection = new Vector2(0, 1);
            if (i == 2)
                OffsetCorrection = new Vector2(1, 0);
            if (i == 3)
                OffsetCorrection = new Vector2(0, 0);
                
            // Here we're essentially checking a square of radius {radius} and seeing if the cell is within a circle from point
            for (int y = (int)Centre.y - SquaresDown; y <= (int)Centre.y + SquaresUp; y++)
            {
                for (int x = (int)Centre.x - SquaresLeft; x <= (int)Centre.x + SquaresRight; x++)
                {
                    Vector2 pin = new Vector2(x, y);
                    if (inside_circle(Centre - OffsetCorrection, pin, r))
                    {
                        points.Add(pin);
                    }
                }
            }
        }
    }

    public void DrawCircleFromCell(Vector2 Centre, int radius, ref List<Vector2> points)
    {
        // This will exclude the centre point from the radius calculation so you'll always select the centre point
        int r = radius;
        
        // Here we're essentially checking a square of radius {radius} and seeing if the cell is within a circle from point
        for (int y = (int)Centre.y - radius; y <= (int)Centre.y + radius; y++)
        {
            for (int x = (int)Centre.x - radius; x <= (int)Centre.x + radius; x++)
            {
                Vector2 pin = new Vector2(x, y);
                if (inside_circle(Centre, pin, r))
                {
                    points.Add(pin);
                }
            }
        }
        
    }
    
    // Useful method to check if a tile is within a given circle radius of given point
    bool inside_circle(Vector2 center, Vector2 tile, float radius) {
        float dx = center.x - tile.x,
            dy = center.y - tile.y;
        float distance_squared = dx*dx + dy*dy;
        return distance_squared <= radius*radius;
    }
}
