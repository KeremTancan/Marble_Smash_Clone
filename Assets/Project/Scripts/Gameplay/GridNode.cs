using UnityEngine;

/// Oyun ızgarasındaki tek bir noktayı temsil eder.
/// Kendi pozisyonunu, dolu olup olmadığını ve üzerindeki topu bilir.

public class GridNode : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    
    public bool IsOccupied { get; private set; }
    
    public void Initialize(Vector2Int gridPosition)
    {
        this.GridPosition = gridPosition;
        this.IsOccupied = false;
        gameObject.name = $"Node ({gridPosition.x}, {gridPosition.y})";
    }

    public void SetOccupied(/* Marble marble */)
    {
        IsOccupied = true;
        // PlacedMarble = marble;
    }

    public void SetVacant()
    {
        IsOccupied = false;
        // PlacedMarble = null;
    }
}