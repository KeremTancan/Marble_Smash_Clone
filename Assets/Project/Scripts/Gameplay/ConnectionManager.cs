using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConnectionManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ObjectPooler pipePooler; 

    private Dictionary<string, PipeConnector> _activePipes = new Dictionary<string, PipeConnector>();
    private HashSet<GridNode> _animatedNodesThisFrame = new HashSet<GridNode>();
    private readonly List<GridNode> _reusableNeighborList = new List<GridNode>(6);
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
                    
                    gridManager.GetNeighbors(currentNode, _reusableNeighborList);
                    foreach (var neighbor in _reusableNeighborList)
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
    
    #region Değişmeyen Kodlar
    public void UpdateAllConnections()
    {
        if (gridManager == null || pipePooler == null) return;
        
        var grid = gridManager.GetGrid();
        if (grid == null) return;
        
        _animatedNodesThisFrame.Clear();
        HashSet<string> requiredConnections = FindAllRequiredConnections(grid);
        
        var keysToRemove = _activePipes.Keys.Except(requiredConnections).ToList();
        foreach (var key in keysToRemove)
        {
            if (_activePipes.TryGetValue(key, out PipeConnector pipeToReturn))
            {
                pipePooler.ReturnObjectToPool(pipeToReturn.gameObject);
            }
            _activePipes.Remove(key);
        }
        
        foreach (var key in requiredConnections)
        {
            if (!_activePipes.ContainsKey(key))
            {
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
    private void AnimatePipeConnection(GridNode from, GridNode to, string key)
    {
        GameObject pipeObject = pipePooler.GetObjectFromPool();
        PipeConnector pipe = pipeObject.GetComponent<PipeConnector>();

        pipe.gameObject.name = $"PipeConn_{key}";
        pipe.AnimateConnection(from.transform.position, to.transform.position, from.PlacedMarble.MarbleColor);
        _activePipes.Add(key, pipe);

        TriggerConnectionJuice(from);
        TriggerConnectionJuice(to);
    }
    private void TriggerConnectionJuice(GridNode node)
    {
        if (_animatedNodesThisFrame.Contains(node)) return;
        
        if (node.PlacedMarble != null)
        {
            var juiceController = node.PlacedMarble.GetComponent<JuiceController>();
            if (juiceController != null)
            {
                juiceController.PlayConnectionBounce();
                
                _animatedNodesThisFrame.Add(node);
            }
        }
    }
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
    #endregion
}