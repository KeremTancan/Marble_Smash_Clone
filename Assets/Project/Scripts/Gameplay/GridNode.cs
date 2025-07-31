using UnityEngine;

public class GridNode : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public bool IsOccupied { get; private set; }
    public Marble PlacedMarble { get; private set; }

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    public void Initialize(Vector2Int gridPosition)
    {
        this.GridPosition = gridPosition;
        this.IsOccupied = false;
        gameObject.name = $"Node ({gridPosition.x}, {gridPosition.y})";
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
    }
    
    public void SetHighlightColor(Color color)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = color;
        }
    }
    public void ResetColor()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    public void SetOccupied(Marble marble)
    {
        IsOccupied = true;
        PlacedMarble = marble;
    }

    public void SetVacant()
    {
        IsOccupied = false;
        PlacedMarble = null;
    }
}