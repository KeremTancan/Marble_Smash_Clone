using System.Collections.Generic;
using UnityEngine;

/// Oyun ızgarasını yönetir. Seviye verisine göre ızgarayı ve aralarındaki
/// görsel bağlantıları oluşturur, node'ları saklar ve merkezler.

public class GridManager : MonoBehaviour
{
    [Header("Seviye Verisi")]
    [SerializeField] private LevelData_SO currentLevelData;

    [Header("Prefabs")]
    [SerializeField] private GridNode gridNodePrefab;
    [SerializeField] private LineRenderer connectionPrefab;

    [Header("Izgara Ayarları")]
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;

    private readonly Dictionary<Vector2Int, GridNode> _grid = new Dictionary<Vector2Int, GridNode>();
    private Transform _connectionsParent;

    private void Start()
    {
        if (currentLevelData == null || gridNodePrefab == null || connectionPrefab == null)
        {
            Debug.LogError("GridManager'da gerekli prefab veya veri dosyaları atanmamış!", this);
            return;
        }
        
        GenerateGrid();
    }
    private void GenerateGrid()
    {
        ClearGrid();

        _connectionsParent = new GameObject("Connections").transform;
        _connectionsParent.SetParent(this.transform);

        for (int y = 0; y < currentLevelData.GridDimensions.y; y++)
        {
            for (int x = 0; x < currentLevelData.GridDimensions.x; x++)
            {
                var gridPos = new Vector2Int(x, y);

                if (currentLevelData.DisabledNodes.Contains(gridPos))
                {
                    continue;
                }

                float worldX = x * horizontalSpacing + (y % 2 != 0 ? horizontalSpacing / 2f : 0);
                float worldY = y * verticalSpacing;
                var worldPosition = new Vector3(worldX, worldY, 0);

                GridNode newNode = Instantiate(gridNodePrefab, worldPosition, Quaternion.identity, this.transform);
                newNode.Initialize(gridPos);
                _grid.Add(gridPos, newNode);
            }
        }
        
        CenterGrid();
        foreach (GridNode node in _grid.Values)
        {
            ConnectToNeighbors(node);
        }
    }

    private void ConnectToNeighbors(GridNode node)
    {
        var pos = node.GridPosition;
        
        var neighborOffsets = new List<Vector2Int>
        {
            new Vector2Int(-1, 0), // Sol
        };
        
        if (pos.y % 2 == 0)
        {
            neighborOffsets.Add(new Vector2Int(-1, -1));
            neighborOffsets.Add(new Vector2Int(0, -1));
        }
        else
        {
            neighborOffsets.Add(new Vector2Int(0, -1));
            neighborOffsets.Add(new Vector2Int(1, -1));
        }

        foreach (var offset in neighborOffsets)
        {
            if (_grid.TryGetValue(pos + offset, out GridNode neighbor))
            {
                CreateConnection(node, neighbor);
            }
        }
    }

    private void CreateConnection(GridNode from, GridNode to)
    {
        LineRenderer line = Instantiate(connectionPrefab, _connectionsParent);
        
        line.positionCount = 2;
        line.SetPosition(0, from.transform.position);
        line.SetPosition(1, to.transform.position);
        
        line.gameObject.name = $"Conn_{from.GridPosition}_{to.GridPosition}";
    }

    private void ClearGrid()
    {
        if (_connectionsParent != null)
        {
            Destroy(_connectionsParent.gameObject);
        }
        foreach (var node in _grid.Values)
        {
            if(node != null) Destroy(node.gameObject);
        }
        _grid.Clear();
    }

    private void CenterGrid()
    {
        if (_grid.Count == 0) return;

        Vector3 centerOffset = Vector3.zero;
        foreach (var node in _grid.Values)
        {
            centerOffset += node.transform.position;
        }
        
        transform.position = -(centerOffset / _grid.Count);
    }
}
