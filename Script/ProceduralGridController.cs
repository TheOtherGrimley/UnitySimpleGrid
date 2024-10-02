using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGridController : MonoBehaviour
{
    [SerializeField] private GameObject CellPrefab;
    [SerializeField] private GameObject LinesPrefab;
    
    [Tooltip("The size of each individual cell")]
    [SerializeField] private float _cellSize = 0.1f;

    [Tooltip("The distance between each cell")]
    [SerializeField] private float _cellPadding = 0.1f;

    [SerializeField] private Vector2 GridSize;

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
                Vector3 offset = new Vector3((x * d), 0f, (y * d));
                GameObject CellPlane = Instantiate(CellPrefab, this.transform.position + offset,
                    Quaternion.identity, transform);
                Cell t_cell = CellPlane.GetComponent<Cell>();
                
                t_cell.CellPlane = CellPlane;
                t_cell.CellPlane.transform.localScale = new Vector3(_cellSize, 1f, _cellSize);
                t_cell.GridCoord = new Vector2(x, y);
                
                _cells[t_cell.GridCoord] = t_cell;
            }
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
        // Draw the grid outline when selected
        Gizmos.DrawLine(transform.position, transform.position + transform.right * ((GridSize.x-1) * (_cellSize*10 + _cellPadding)));
        Gizmos.DrawLine(transform.position + transform.right * ((GridSize.x-1) * (_cellSize*10 + _cellPadding)), (transform.position + transform.right * ((GridSize.x-1) * (_cellSize*10 + _cellPadding))) + transform.forward * ((GridSize.y-1) * (_cellSize*10 + _cellPadding)));
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * ((GridSize.y-1) * (_cellSize*10 + _cellPadding)));
        Gizmos.DrawLine(transform.position + transform.forward * ((GridSize.y-1) * (_cellSize*10 + _cellPadding)), transform.position + transform.forward * ((GridSize.y-1) * (_cellSize*10 + _cellPadding)) + transform.right * ((GridSize.x-1) * (_cellSize*10 + _cellPadding)));
    }
}
