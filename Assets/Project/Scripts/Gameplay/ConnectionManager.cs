using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConnectionManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PipeConnector pipePrefab;

    private Transform _connectionsParent;
    
    private Dictionary<string, PipeConnector> _activePipes = new Dictionary<string, PipeConnector>();

    void Start()
    {
        _connectionsParent = new GameObject("Marble_Connections").transform;
        _connectionsParent.SetParent(this.transform);
    }
    
    public void UpdateAllConnections()
    {
        if (gridManager == null) return;
        var grid = gridManager.GetGrid();
        if (grid == null) return;

        HashSet<string> requiredConnections = FindAllRequiredConnections(grid);

        var keysToRemove = _activePipes.Keys.Except(requiredConnections).ToList();
        foreach (var key in keysToRemove)
        {
            if (_activePipes.TryGetValue(key, out PipeConnector pipeToDestroy))
            {
                Destroy(pipeToDestroy.gameObject);
            }
            _activePipes.Remove(key);
        }

        // 3. Henüz oluşturulmamış YENİ bağlantıları bul, oluştur ve canlandır
        foreach (var key in requiredConnections)
        {
            if (!_activePipes.ContainsKey(key))
            {
                // Anahtardan nodeları geri çöz
                string[] positions = key.Split('_');
                Vector2Int pos1 = StringToVector2Int(positions[0]);
                Vector2Int pos2 = StringToVector2Int(positions[1]);

                if (grid.TryGetValue(pos1, out GridNode node1) && grid.TryGetValue(pos2, out GridNode node2))
                {
                    AnimatePipeConnection(node1, node2, key);
                }
            }
        }
    }

    private HashSet<string> FindAllRequiredConnections(Dictionary<Vector2Int, GridNode> grid)
    {
        var requiredKeys = new HashSet<string>();
        var visitedNodes = new HashSet<GridNode>();

        foreach (var node in grid.Values)
        {
            if (node.IsOccupied && !visitedNodes.Contains(node))
            {
                Stack<GridNode> stack = new Stack<GridNode>();
                stack.Push(node);
                visitedNodes.Add(node);

                while (stack.Count > 0)
                {
                    var currentNode = stack.Pop();
                    foreach (var neighbor in gridManager.GetNeighbors(currentNode))
                    {
                        if (neighbor.IsOccupied && 
                            !visitedNodes.Contains(neighbor) && 
                            neighbor.PlacedMarble.MarbleColor == currentNode.PlacedMarble.MarbleColor)
                        {
                            visitedNodes.Add(neighbor);
                            stack.Push(neighbor);
                            requiredKeys.Add(GetConnectionKey(currentNode, neighbor));
                        }
                    }
                }
            }
        }
        return requiredKeys;
    }

    // İki node arasına boru çizen ve sözlüğe ekleyen metot
    private void AnimatePipeConnection(GridNode from, GridNode to, string key)
    {
        PipeConnector pipe = Instantiate(pipePrefab, _connectionsParent);
        pipe.gameObject.name = $"PipeConn_{key}";
        pipe.AnimateConnection(from.transform.position, to.transform.position, from.PlacedMarble.MarbleColor);
        _activePipes.Add(key, pipe);
    }
    
    // İki node'dan her zaman aynı sırada (küçükten büyüğe) bir anahtar üretir
    // Bu, A-B ve B-A bağlantılarının aynı kabul edilmesini sağlar
    private string GetConnectionKey(GridNode nodeA, GridNode nodeB)
    {
        if (nodeA.GetInstanceID() < nodeB.GetInstanceID())
            return $"{nodeA.GridPosition.x},{nodeA.GridPosition.y}_{nodeB.GridPosition.x},{nodeB.GridPosition.y}";
        else
            return $"{nodeB.GridPosition.x},{nodeB.GridPosition.y}_{nodeA.GridPosition.x},{nodeA.GridPosition.y}";
    }
    private Vector2Int StringToVector2Int(string s)
    {
        var parts = s.Split(',');
        return new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
    }
}