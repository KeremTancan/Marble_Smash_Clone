using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Seviye Verisi")]
    [SerializeField] private LevelData_SO currentLevelData;
    [Header("Prefabs")]
    [SerializeField] private GridNode gridNodePrefab;
    [SerializeField] private LineRenderer connectionPrefab;
    [Header("Yöneticiler")]
    [SerializeField] private ConnectionManager connectionManager; // YENİ EKLENDİ
    [Header("Güvenli Alan Ayarları")]
    [SerializeField] private Rect safeArea = new Rect(-4f, -4f, 8f, 8f);
    [Header("Izgara Ayarları")]
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;
    
    private readonly Dictionary<Vector2Int, GridNode> _grid = new Dictionary<Vector2Int, GridNode>();
    private Transform _connectionsParent;

    // ConnectionManager'ın grid verisine erişmesi için public metot
    public Dictionary<Vector2Int, GridNode> GetGrid() => _grid;

    public GridNode GetClosestNode(Vector3 worldPosition)
    {
        if (_grid.Count == 0) return null;

        GridNode closestNode = null;
        float minDistanceSqr = float.MaxValue;

        foreach (GridNode node in _grid.Values)
        {
            float distanceSqr = (node.transform.position - worldPosition).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                closestNode = node;
            }
        }
        
        float threshold = horizontalSpacing * 1.5f;
        if (minDistanceSqr > threshold * threshold)
        {
            return null;
        }

        return closestNode;
    }

    public Dictionary<Marble, GridNode> GetTargetPlacement(Shape shape)
    {
        var placement = new Dictionary<Marble, GridNode>();
        var marbles = shape.GetMarbles();
        if (marbles.Count == 0) return placement;

        Marble centerMarble = marbles[0];
        GridNode anchorNode = GetClosestNode(centerMarble.transform.position);
        
        if (anchorNode == null) return placement;

        Vector3 anchorMarblePos = centerMarble.transform.position;
        for (int i = 0; i < marbles.Count; i++)
        {
            Marble currentMarble = marbles[i];
            Vector3 offset = currentMarble.transform.position - anchorMarblePos;
            Vector3 targetWorldPos = anchorNode.transform.position + offset;

            GridNode targetNode = FindNearestNodeUnconditionally(targetWorldPos);
            if (targetNode != null)
            {
                placement[currentMarble] = targetNode;
            }
        }
        return placement;
    }
    
    private GridNode FindNearestNodeUnconditionally(Vector3 worldPosition)
    {
        if (_grid.Count == 0) return null;
        GridNode closestNode = null;
        float minDistanceSqr = float.MaxValue;
        foreach (GridNode node in _grid.Values)
        {
            float distanceSqr = (node.transform.position - worldPosition).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                closestNode = node;
            }
        }
        return closestNode;
    }


    public bool CheckPlacementValidity(Dictionary<Marble, GridNode> placement)
    {
        var targetNodes = placement.Values.ToList();
        if (targetNodes.Count != targetNodes.Distinct().Count())
        {
            return false;
        }
        foreach (var node in targetNodes)
        {
            if (node.IsOccupied) return false;
        }
        return true;
    }
    
    public void PlaceShape(Shape shape, Dictionary<Marble, GridNode> placement)
    {
        foreach (var pair in placement)
        {
            Marble marbleToMove = pair.Key;
            GridNode destinationNode = pair.Value;

            marbleToMove.transform.position = destinationNode.transform.position;
            marbleToMove.transform.SetParent(this.transform, true);
            destinationNode.SetOccupied(marbleToMove);
        }
        Destroy(shape.gameObject);
        
        foreach (var pair in placement)
        {
            CheckForMatches(pair.Value);
        }

        // Toplar yerleştirildikten sonra tüm bağlantıları güncelle
        connectionManager.UpdateAllConnections(); // YENİ EKLENDİ
    }
    
    #region Match & Explosion Logic
    private void CheckForMatches(GridNode startNode)
    {
        if (startNode == null || !startNode.IsOccupied)
            return;

        List<GridNode> connectedNodes = new List<GridNode>();
        Queue<GridNode> nodesToVisit = new Queue<GridNode>();
        HashSet<GridNode> visitedNodes = new HashSet<GridNode>();

        Color matchColor = startNode.PlacedMarble.MarbleColor;
        nodesToVisit.Enqueue(startNode);
        visitedNodes.Add(startNode);
        
        while (nodesToVisit.Count > 0)
        {
            GridNode currentNode = nodesToVisit.Dequeue();
            connectedNodes.Add(currentNode);

            foreach (GridNode neighbor in GetNeighbors(currentNode))
            {
                if (neighbor != null && neighbor.IsOccupied && !visitedNodes.Contains(neighbor) && neighbor.PlacedMarble.MarbleColor == matchColor)
                {
                    visitedNodes.Add(neighbor);
                    nodesToVisit.Enqueue(neighbor);
                }
            }
        }
        
        if (connectedNodes.Count >= 5)
        {
            foreach (GridNode node in connectedNodes)
            {
                if (node.PlacedMarble != null)
                {
                    Destroy(node.PlacedMarble.gameObject);
                    node.SetVacant();
                }
            }
            Debug.Log(connectedNodes.Count + " adet top patlatıldı!");

            // Patlamadan sonra bağlantıları tekrar güncelle
            connectionManager.UpdateAllConnections(); // YENİ EKLENDİ
        }
    }

    private List<GridNode> GetNeighbors(GridNode node)
    {
        List<GridNode> neighbors = new List<GridNode>();
        Vector2Int[] neighborOffsets;
        
        if (node.GridPosition.y % 2 == 0)
        {
            neighborOffsets = new Vector2Int[] {
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(0, 1), new Vector2Int(-1, 1),
                new Vector2Int(0, -1), new Vector2Int(-1, -1)
            };
        }
        else
        {
            neighborOffsets = new Vector2Int[] {
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(1, 1), new Vector2Int(0, 1),
                new Vector2Int(1, -1), new Vector2Int(0, -1)
            };
        }

        foreach (var offset in neighborOffsets)
        {
            if (_grid.TryGetValue(node.GridPosition + offset, out GridNode neighborNode))
            {
                neighbors.Add(neighborNode);
            }
        }
        return neighbors;
    }
    #endregion


    #region Unchanged Code
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
        _connectionsParent = new GameObject("Grid_Connections").transform; // İsmi değiştirdim karışmasın diye
        _connectionsParent.SetParent(this.transform);
        for (int y = 0; y < currentLevelData.GridDimensions.y; y++)
        {
            for (int x = 0; x < currentLevelData.GridDimensions.x; x++)
            {
                var gridPos = new Vector2Int(x, y);
                if (currentLevelData.DisabledNodes.Contains(gridPos)) continue;
                float worldX = x * horizontalSpacing + (y % 2 != 0 ? horizontalSpacing / 2f : 0);
                float worldY = y * verticalSpacing;
                var worldPosition = new Vector3(worldX, worldY, 0);
                GridNode newNode = Instantiate(gridNodePrefab, worldPosition, Quaternion.identity, this.transform);
                newNode.Initialize(gridPos);
                _grid.Add(gridPos, newNode);
            }
        }
        FitGridToSafeArea();
        foreach (GridNode node in _grid.Values)
        {
            ConnectToNeighbors(node);
        }
    }
    
    private void FitGridToSafeArea()
    {
        if (_grid.Count == 0) return;
        Bounds gridBounds = GetGridBounds();
        float widthScale = gridBounds.size.x > 0 ? safeArea.width / gridBounds.size.x : 1f;
        float heightScale = gridBounds.size.y > 0 ? safeArea.height / gridBounds.size.y : 1f;
        float scale = Mathf.Min(widthScale, heightScale);
        if (scale < 1.0f)
        {
            transform.localScale = Vector3.one * scale;
        }
        
        gridBounds = GetGridBounds();
        Vector3 finalCenterOffset = gridBounds.center - (Vector3)safeArea.center;
        transform.position -= finalCenterOffset;
    }

    private Bounds GetGridBounds()
    {
        if (_grid.Count == 0) return new Bounds();
        var firstNodePos = _grid.Values.First().transform.position;
        var bounds = new Bounds(firstNodePos, Vector3.zero);
        foreach (var node in _grid.Values)
        {
            bounds.Encapsulate(node.transform.position);
        }
        return bounds;
    }

    private void ConnectToNeighbors(GridNode node)
    {
        var pos = node.GridPosition;
        var neighborOffsets = new List<Vector2Int> { new Vector2Int(-1, 0) };
        if (pos.y % 2 == 0) {
            neighborOffsets.Add(new Vector2Int(-1, -1));
            neighborOffsets.Add(new Vector2Int(0, -1));
        } else {
            neighborOffsets.Add(new Vector2Int(0, -1));
            neighborOffsets.Add(new Vector2Int(1, -1));
        }
        foreach (var offset in neighborOffsets) {
            if (_grid.TryGetValue(pos + offset, out GridNode neighbor)) {
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
        line.gameObject.name = $"GridConn_{from.GridPosition}_{to.GridPosition}";
    }

    private void ClearGrid()
    {
        transform.localScale = Vector3.one;
        transform.position = Vector3.zero;
        if (_connectionsParent != null) Destroy(_connectionsParent.gameObject);
        foreach (var node in _grid.Values) if(node != null) Destroy(node.gameObject);
        _grid.Clear();
    }
    #endregion
}