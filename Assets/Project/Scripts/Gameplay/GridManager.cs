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
    [Header("Güvenli Alan Ayarları")]
    [SerializeField] private Rect safeArea = new Rect(-4f, -4f, 8f, 8f);
    [Header("Izgara Ayarları")]
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;
    
    private readonly Dictionary<Vector2Int, GridNode> _grid = new Dictionary<Vector2Int, GridNode>();
    private Transform _connectionsParent;

    public GridNode GetNodeAtWorldPosition(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        int y = Mathf.RoundToInt(localPos.y / verticalSpacing);
        float xOffset = (y % 2 != 0) ? 0.5f : 0;
        int x = Mathf.RoundToInt(localPos.x / horizontalSpacing - xOffset);
        _grid.TryGetValue(new Vector2Int(x, y), out GridNode node);
        return node;
    }

    public List<GridNode> GetTargetNodesForShape(Shape shape, GridNode anchorNode)
    {
        var targetNodes = new List<GridNode>();
        if (anchorNode == null) return targetNodes;

        Vector2Int rootShapePos = shape.ShapeData.MarblePositions.First();
        Vector2Int shapeRootGridPos = anchorNode.GridPosition - rootShapePos;

        foreach (var marblePos in shape.ShapeData.MarblePositions)
        {
            Vector2Int targetGridPos = shapeRootGridPos + marblePos;
            if (_grid.TryGetValue(targetGridPos, out GridNode node))
            {
                targetNodes.Add(node);
            }
        }
        return targetNodes;
    }

    public bool CheckPlacementValidity(Shape shape, List<GridNode> targetNodes)
    {
        if (targetNodes.Count < shape.GetMarbles().Count) return false;
        foreach (var node in targetNodes)
        {
            if (node.IsOccupied) return false;
        }
        return true;
    }
    
    public void PlaceShape(Shape shape, List<GridNode> targetNodes)
    {
        var marblesToPlace = shape.GetMarbles();
        for (int i = 0; i < targetNodes.Count; i++)
        {
            Marble marbleToMove = marblesToPlace[i];
            GridNode destinationNode = targetNodes[i];

            marbleToMove.transform.position = destinationNode.transform.position;
            marbleToMove.transform.SetParent(this.transform, true);
            destinationNode.SetOccupied(marbleToMove);
        }
        Destroy(shape.gameObject);
    }

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
        _connectionsParent = new GameObject("Connections").transform;
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
        line.gameObject.name = $"Conn_{from.GridPosition}_{to.GridPosition}";
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
