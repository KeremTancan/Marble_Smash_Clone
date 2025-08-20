using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Seviye Verisi")] [SerializeField]
    private LevelData_SO currentLevelData;

    [Header("Prefabs")] [SerializeField] private GridNode gridNodePrefab;
    [SerializeField] private LineRenderer connectionPrefab;
    [SerializeField] private GameObject marblePrefab;

    [Header("Yöneticiler")] [SerializeField]
    private ConnectionManager connectionManager;
    [SerializeField] private FXManager fxManager;

    [Header("Güvenli Alan Ayarları")] [SerializeField]
    private Rect safeArea = new Rect(-4f, -4f, 8f, 8f);

    [Header("Izgara Ayarları")] [SerializeField]
    private float horizontalSpacing = 1.0f;

    [SerializeField] private float verticalSpacing = 0.866f;

    [Header("Roket Ayarları")] [SerializeField]
    private Rocket rocketPrefab;
    
    private Queue<Rocket> _rocketPool = new Queue<Rocket>();
    private List<Rocket> _activeRockets = new List<Rocket>();

    private readonly Dictionary<Vector2Int, GridNode> _grid = new Dictionary<Vector2Int, GridNode>();
    private Transform _connectionsParent;
   private readonly List<GridNode> _reusableNeighborList = new List<GridNode>(6);

    public Dictionary<Vector2Int, GridNode> GetGrid() => _grid;
   public void GetNeighbors(GridNode node, List<GridNode> neighborsList)
    {
        neighborsList.Clear(); 
        Vector2Int[] neighborOffsets;
        if (node.GridPosition.y % 2 == 0)
        {
            neighborOffsets = new Vector2Int[]
            {
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(0, 1), new Vector2Int(-1, 1),
                new Vector2Int(0, -1), new Vector2Int(-1, -1)
            };
        }
        else
        {
            neighborOffsets = new Vector2Int[]
            {
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(1, 1), new Vector2Int(0, 1),
                new Vector2Int(1, -1), new Vector2Int(0, -1)
            };
        }

        foreach (var offset in neighborOffsets)
        {
            if (_grid.TryGetValue(node.GridPosition + offset, out GridNode neighborNode))
            {
                neighborsList.Add(neighborNode);
            }
        }
    }
    
    public Dictionary<Color, int> GetNeighboringColors(List<GridNode> placementNodes)
    {
        var colorCounts = new Dictionary<Color, int>();
        var checkedNeighbors = new HashSet<GridNode>();

        foreach (var node in placementNodes)
        {
            // DÜZELTME 2: Script içindeki çağrıyı yeni metoda göre güncelle
            GetNeighbors(node, _reusableNeighborList);
            foreach (var neighbor in _reusableNeighborList)
            {
                if (neighbor.IsOccupied && !checkedNeighbors.Contains(neighbor) && !placementNodes.Contains(neighbor))
                {
                    Color color = neighbor.PlacedMarble.MarbleColor;
                    if (!colorCounts.ContainsKey(color))
                        colorCounts[color] = 0;

                    colorCounts[color]++;
                    checkedNeighbors.Add(neighbor);
                }
            }
        }

        return colorCounts;
    }
    
    private List<GridNode> FindConnectedGroup(GridNode startNode)
    {
        var connectedNodes = new List<GridNode>();
        if (startNode == null || !startNode.IsOccupied)
            return connectedNodes;

        var nodesToVisit = new Queue<GridNode>();
        var visitedNodes = new HashSet<GridNode>();
        var matchColor = startNode.PlacedMarble.MarbleColor;

        nodesToVisit.Enqueue(startNode);
        visitedNodes.Add(startNode);

        while (nodesToVisit.Count > 0)
        {
            GridNode currentNode = nodesToVisit.Dequeue();
            connectedNodes.Add(currentNode);
            GetNeighbors(currentNode, _reusableNeighborList);
            foreach (GridNode neighbor in _reusableNeighborList)
            {
                if (neighbor != null && neighbor.IsOccupied && !visitedNodes.Contains(neighbor) &&
                    neighbor.PlacedMarble.MarbleColor == matchColor)
                {
                    visitedNodes.Add(neighbor);
                    nodesToVisit.Enqueue(neighbor);
                }
            }
        }

        return connectedNodes;
    }

    public void PlaceShape(Shape shape, Dictionary<Marble, GridNode> placement)
    {
        List<GridNode> placedNodes = new List<GridNode>();
        foreach (var pair in placement)
        {
            Marble marbleToMove = pair.Key;
            GridNode destinationNode = pair.Value;

            marbleToMove.transform.position = destinationNode.transform.position;
            marbleToMove.transform.SetParent(this.transform, true);
            destinationNode.SetOccupied(marbleToMove);
            placedNodes.Add(destinationNode);
            
            var juiceController = marbleToMove.GetComponent<JuiceController>();
            if (juiceController != null)
            {
                juiceController.PlayPlacementAnimation();
            }
        }
        AudioManager.Instance.PlayMarblePlaceSound();
        Destroy(shape.gameObject);
        StartCoroutine(ProcessPlacementSequence(placedNodes));
    }
    private readonly WaitForSeconds _placementDelay = new WaitForSeconds(0.2f);
    private IEnumerator ProcessPlacementSequence(List<GridNode> placedNodes)
    {
        yield return _placementDelay;
        connectionManager.UpdateAllConnections();
        yield return StartCoroutine(ProcessMatchesAfterPlacement(placedNodes));
    }

    private IEnumerator ProcessMatchesAfterPlacement(List<GridNode> placedNodes)
    {
        HashSet<GridNode> allNodesToExplode = new HashSet<GridNode>();
        bool explosionOccurred = false;

        foreach (var startNode in placedNodes)
        {
            if (startNode == null || !startNode.IsOccupied || allNodesToExplode.Contains(startNode))
                continue;

            List<GridNode> connectedGroup = FindConnectedGroup(startNode);
            
            if (connectedGroup.Count >= 5)
            {
                explosionOccurred = true;
                foreach (var node in connectedGroup)
                {
                    allNodesToExplode.Add(node);
                }
            }
        }
        
        if (explosionOccurred)
        {
            yield return StartCoroutine(PlayExplosionAnimations(allNodesToExplode));
            EventManager.RaiseOnMarblesExploded(allNodesToExplode.Count);
            Debug.Log(allNodesToExplode.Count + " adet top patlatıldı!");
            connectionManager.UpdateAllConnections();
        }
        
        EventManager.RaiseOnTurnCompleted();
    }
    
    private IEnumerator PlayExplosionAnimations(HashSet<GridNode> nodesToExplode)
    {
        int animationsPending = nodesToExplode.Count;

        foreach (GridNode node in nodesToExplode)
        {
            if (node.PlacedMarble != null)
            {
                Marble marble = node.PlacedMarble;
                GridNode capturedNode = node; 

                marble.PlayExplosionAnimation(() => {
                    fxManager.PlayExplosionEffect(capturedNode.transform.position, marble.MarbleColor);
                    Destroy(marble.gameObject);
                    capturedNode.SetVacant();
                    animationsPending--; 
                    AudioManager.Instance.PlayExplosionSound();
                    AudioManager.Instance.TriggerHaptics();
                });
            }
            else
            {
                animationsPending--;
            }
        }
        yield return new WaitUntil(() => animationsPending == 0);
    }
    
    #region Değişmeyen Kodlar
    public void LaunchFireworksFromNode(GridNode startNode)
    {
        if (rocketPrefab == null)
        {
            Debug.LogError("GridManager'a Roket Prefab'ı atanmamış!");
            return;
        }

        Debug.Log($"{startNode.GridPosition} noktasından roketler fırlatılıyor!");

        Vector2Int[] diagonalOffsets;
        if (startNode.GridPosition.y % 2 == 0) // Çift satırlar için çapraz yönler
        {
            diagonalOffsets = new Vector2Int[]
            {
                new Vector2Int(-1, 1), // Sol-üst
                new Vector2Int(0, 1), // Sağ-üst
                new Vector2Int(-1, -1), // Sol-alt
                new Vector2Int(0, -1) // Sağ-alt
            };
        }
        else
        {
            diagonalOffsets = new Vector2Int[]
            {
                new Vector2Int(0, 1), // Sol-üst
                new Vector2Int(1, 1), // Sağ-üst
                new Vector2Int(0, -1), // Sol-alt
                new Vector2Int(1, -1) // Sağ-alt
            };
        }

        // Her bir çapraz yöne bir roket fırlat
        foreach (var offset in diagonalOffsets)
        {
            Vector2Int theoreticalNeighborPos = startNode.GridPosition + offset;
            float worldX = theoreticalNeighborPos.x * horizontalSpacing +
                           (theoreticalNeighborPos.y % 2 != 0 ? horizontalSpacing / 2f : 0);
            float worldY = theoreticalNeighborPos.y * verticalSpacing;
            Vector3 theoreticalWorldPos = transform.TransformPoint(new Vector3(worldX, worldY, 0));
            Vector3 direction = (theoreticalWorldPos - startNode.transform.position).normalized;
            Rocket newRocket = RocketPoolManager.Instance.GetRocket(); 
            newRocket.transform.position = startNode.transform.position; 
            newRocket.Launch(direction, this);
        }
        AudioManager.Instance.PlayExplosionSound();
        ExplodeMarble(startNode.PlacedMarble);
    }
    public void ExplodeMarble(Marble marble)
    {
        if (marble == null || marble.ParentNode == null || !marble.ParentNode.IsOccupied) return;

        GridNode node = marble.ParentNode;

        if (fxManager != null)
        {
            fxManager.PlayExplosionEffect(node.transform.position, marble.MarbleColor);
        }
        EventManager.RaiseOnMarblesExploded(1);

        Destroy(marble.gameObject);
        node.SetVacant();
        connectionManager.UpdateAllConnections();
    }
    public List<GridNode> GetPlacementNodes(ShapeData_SO shapeData, Vector2Int anchorPosition)
    {
        var nodes = new List<GridNode>();
        foreach (var offset in shapeData.MarblePositions)
        {
            if (_grid.TryGetValue(anchorPosition + offset, out GridNode node))
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }
    public bool CheckPlacementValidity(Dictionary<Marble, GridNode> placement)
    {
        var targetNodes = placement.Values.ToList();
        if (targetNodes.Count != placement.Count) return false;

        if (targetNodes.Count != targetNodes.Distinct().Count())
        {
            return false;
        }

        foreach (var node in targetNodes)
        {
            if (node == null || !node.IsAvailable) return false;
        }

        return true;
    }
    public bool CanShapeBePlacedAnywhere(ShapeData_SO shapeData)
    {
        var shapeOffsets = shapeData.MarblePositions;
        if (shapeOffsets == null || shapeOffsets.Count == 0) return false;

        foreach (GridNode potentialAnchorNode in _grid.Values)
        {
            foreach (Vector2Int shapeAnchorOffset in shapeOffsets)
            {
                bool isThisEntirePlacementValid = true;
                foreach (Vector2Int marbleOffset in shapeOffsets)
                {
                    Vector2Int targetGridPos = potentialAnchorNode.GridPosition - shapeAnchorOffset + marbleOffset;

                    if (!_grid.TryGetValue(targetGridPos, out GridNode targetNode) || !targetNode.IsAvailable)
                    {
                        isThisEntirePlacementValid = false;
                        break;
                    }
                }

                if (isThisEntirePlacementValid)
                {
                    return true; 
                }
            }
        }

        return false; 
    }
    public void GenerateGrid(LevelData_SO levelData)
    {
        if (levelData == null || gridNodePrefab == null || connectionPrefab == null || marblePrefab == null)
        {
            Debug.LogError("GridManager'da gerekli prefab veya veri dosyaları atanmamış!", this);
            return;
        }

        ClearGrid();
        _connectionsParent = new GameObject("Grid_Connections").transform;
        _connectionsParent.SetParent(this.transform);

        var lockedNodeDictionary = new Dictionary<Vector2Int, int>();
        foreach (var lockedNodeData in levelData.LockedNodes)
        {
            lockedNodeDictionary[lockedNodeData.Position] = lockedNodeData.MarblesToUnlock;
        }

        for (int y = 0; y < levelData.GridDimensions.y; y++)
        {
            for (int x = 0; x < levelData.GridDimensions.x; x++)
            {
                var gridPos = new Vector2Int(x, y);
                if (levelData.DisabledNodes.Contains(gridPos)) continue;

                float worldX = x * horizontalSpacing + (y % 2 != 0 ? horizontalSpacing / 2f : 0);
                float worldY = y * verticalSpacing;
                var worldPosition = new Vector3(worldX, worldY, 0);
                GridNode newNode = Instantiate(gridNodePrefab, worldPosition, Quaternion.identity, this.transform);
                newNode.Initialize(gridPos);
                _grid.Add(gridPos, newNode);

                if (lockedNodeDictionary.TryGetValue(gridPos, out int marblesToUnlock))
                {
                    newNode.Lock(marblesToUnlock);
                }
            }
        }

        FitGridToSafeArea();
        PlacePrePlacedShapes(levelData);
        foreach (GridNode node in _grid.Values)
        {
            ConnectToNeighbors(node);
        }
    }
    private void PlacePrePlacedShapes(LevelData_SO levelData)
    {
        if (levelData.AvailableColors == null || levelData.AvailableColors.Colors.Count == 0) return;

        foreach (var shapeToPlace in levelData.PrePlacedShapes)
        {
            Color randomColor =
                levelData.AvailableColors.Colors[Random.Range(0, levelData.AvailableColors.Colors.Count)];

            foreach (var marbleOffset in shapeToPlace.ShapeData.MarblePositions)
            {
                Vector2Int targetPos = shapeToPlace.AnchorPosition + marbleOffset;
                if (_grid.TryGetValue(targetPos, out GridNode targetNode))
                {
                    if (targetNode.IsAvailable)
                    {
                        GameObject marbleObj = Instantiate(marblePrefab, targetNode.transform.position,
                            Quaternion.identity, this.transform);
                        marbleObj.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);

                        Marble newMarble = marbleObj.GetComponent<Marble>();
                        newMarble.SetColor(randomColor);
                        targetNode.SetOccupied(newMarble);
                    }
                    else
                    {
                        Debug.LogWarning($"Hazır şekil yerleştirilemedi: {targetPos} noktası dolu veya kilitli!");
                    }
                }
            }
        }
    }
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
        if (minDistanceSqr > threshold * threshold) return null;
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
        line.gameObject.name = $"GridConn_{from.GridPosition}_{to.GridPosition}";
    }
    private void ClearGrid()
    {
        transform.localScale = Vector3.one;
        transform.position = Vector3.zero;
        if (_connectionsParent != null) Destroy(_connectionsParent.gameObject);
        foreach (var node in _grid.Values)
            if (node != null)
                Destroy(node.gameObject);
        _grid.Clear();
    }
    #endregion
}