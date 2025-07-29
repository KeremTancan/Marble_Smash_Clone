using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private LineRenderer connectionPrefab;

    private Transform _connectionsParent;

    void Start()
    {
        _connectionsParent = new GameObject("Marble_Connections").transform;
        _connectionsParent.SetParent(this.transform);
    }
    
    public void UpdateAllConnections()
    {
        foreach (Transform child in _connectionsParent)
        {
            Destroy(child.gameObject);
        }

        if (gridManager == null) return;

        var grid = gridManager.GetGrid();
        if (grid == null) return;
        
        foreach (GridNode node in grid.Values)
        {
            if (!node.IsOccupied) continue;
            
            var neighborsToCheck = GetNeighborsToCheck(node);
            foreach (GridNode neighbor in neighborsToCheck)
            {
                if (neighbor.IsOccupied && neighbor.PlacedMarble.MarbleColor == node.PlacedMarble.MarbleColor)
                {
                    DrawConnection(node, neighbor);
                }
            }
        }
    }

    private void DrawConnection(GridNode from, GridNode to)
    {
        LineRenderer line = Instantiate(connectionPrefab, _connectionsParent);
        line.positionCount = 2;
        line.SetPosition(0, from.transform.position);
        line.SetPosition(1, to.transform.position);

        Color marbleColor = from.PlacedMarble.MarbleColor;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        line.GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", marbleColor);
        line.SetPropertyBlock(propBlock);

        line.widthMultiplier = 0.3f; 
        line.sortingOrder = 1; 
        line.numCapVertices = 4; 
        
        line.gameObject.name = $"MarbleConn_{from.GridPosition}_{to.GridPosition}";
    }
    
    private List<GridNode> GetNeighborsToCheck(GridNode node)
    {
        var neighbors = new List<GridNode>();
        Vector2Int[] offsets;

        if (node.GridPosition.y % 2 == 0)
        {
            offsets = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1) };
        }
        else
        {
            offsets = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1) };
        }

        foreach (var offset in offsets)
        {
            if (gridManager.GetGrid().TryGetValue(node.GridPosition + offset, out var neighbor))
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }
}