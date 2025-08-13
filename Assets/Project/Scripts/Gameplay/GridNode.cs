using UnityEngine;
using UnityEngine.UI;

public class GridNode : MonoBehaviour
{
    [Header("Kilit Görseli Referansları")]
    [SerializeField] private GameObject lockVisual;
    [SerializeField] private Text lockValueText;

    public Vector2Int GridPosition { get; private set; }
    public bool IsOccupied { get; private set; }
    public Marble PlacedMarble { get; private set; }
    public bool IsLocked { get; private set; }
    public int MarblesToUnlock { get; private set; }
    public bool IsAvailable => !IsOccupied && !IsLocked && gameObject.activeSelf;
    
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    
    public void Initialize(Vector2Int gridPosition)
    {
        this.GridPosition = gridPosition;
        gameObject.name = $"Node ({gridPosition.x}, {gridPosition.y})";
        
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null) _originalColor = _spriteRenderer.color;

        if (lockVisual != null) lockVisual.SetActive(false);
    }
    
    public void Lock(int marblesToUnlock)
    {
        IsLocked = true;
        MarblesToUnlock = marblesToUnlock;
        if (lockVisual != null) lockVisual.SetActive(true);
        if (lockValueText != null) lockValueText.text = MarblesToUnlock.ToString();
    }

    public void Unlock()
    {
        if (!IsLocked) return;
        
        IsLocked = false;
        if (lockVisual != null) lockVisual.SetActive(false);
        Debug.Log($"Node {GridPosition} kilidi açıldı!");
    }

    public void SetHighlightColor(Color color) { if (_spriteRenderer != null) _spriteRenderer.color = color; }
    public void ResetColor() { if (_spriteRenderer != null) _spriteRenderer.color = _originalColor; }
    public void SetOccupied(Marble marble) {
        IsOccupied = true;
        PlacedMarble = marble;
        if (marble != null) marble.ParentNode = this;
    }
    public void SetVacant() {
        IsOccupied = false;
        PlacedMarble = null;
    }
}