using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [Header("Görsel Efektler")]
    [SerializeField] private float pickupScale = 1.2f;
    [SerializeField] private float scaleDuration = 0.1f;
    [SerializeField] private Vector3 pickupOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private GhostController ghostPrefab;

    public ShapeData_SO ShapeData { get; private set; }
    public bool IsPlaced { get; private set; }
    public Transform OriginalParent { get; private set; }

    private List<Marble> _marbles = new List<Marble>();
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private GridManager _gridManager;
    private GhostController _ghostInstance;
    private Coroutine _activeCoroutine;
    private Dictionary<Marble, GridNode> _lastValidPlacement;
    
    private List<GridNode> _lastMarkedNodes = new List<GridNode>();

    private void Awake()
    {
        _originalScale = transform.localScale;
        _gridManager = FindObjectOfType<GridManager>();
    }

    public void OnSelected()
    {
        _originalPosition = transform.position;
        OriginalParent = transform.parent;
        
        if (_ghostInstance == null)
        {
            _ghostInstance = Instantiate(ghostPrefab);
            _ghostInstance.Initialize(this);
        }
        _ghostInstance.gameObject.SetActive(false);
        ResetMarkedNodes();

        RunCoroutine(PickupRoutine());
    }

    public void OnDrag(Vector3 newPosition)
    {
        transform.position = newPosition + pickupOffset;
        ResetMarkedNodes();

        var targetPlacement = _gridManager.GetTargetPlacement(this);
        
        if (targetPlacement.Count > 0)
        {
            bool isValid = _gridManager.CheckPlacementValidity(targetPlacement);
            
            if (isValid)
            {
                _ghostInstance.gameObject.SetActive(true);
                _ghostInstance.UpdatePositions(targetPlacement);
                _lastValidPlacement = targetPlacement;
            }
            else
            {
                _ghostInstance.gameObject.SetActive(false);
                MarkNodes(targetPlacement.Values, Color.red);
                _lastValidPlacement = null;
            }
        }
        else
        {
            _ghostInstance.gameObject.SetActive(false);
            _lastValidPlacement = null;
        }
    }

    public void OnDropped()
    {
        ResetMarkedNodes();

        if (_ghostInstance != null)
        {
            _ghostInstance.gameObject.SetActive(false);
        }

        if (_lastValidPlacement != null)
        {
            _gridManager.PlaceShape(this, _lastValidPlacement);
            IsPlaced = true;
            this.enabled = false;
        }
        else
        {
            RunCoroutine(ReturnRoutine());
        }
    }
    
    private void MarkNodes(IEnumerable<GridNode> nodes, Color color)
    {
        foreach (var node in nodes)
        {
            node.SetHighlightColor(color);
            _lastMarkedNodes.Add(node);
        }
    }

    private void ResetMarkedNodes()
    {
        foreach (var node in _lastMarkedNodes)
        {
            node.ResetColor();
        }
        _lastMarkedNodes.Clear();
    }
    
    #region Unchanged Code
    public List<Marble> GetMarbles() => _marbles;
    public void Initialize(ShapeData_SO shapeData, ColorPalette_SO palette, GameObject marblePrefab, float hSpacing, float vSpacing){
        this.ShapeData = shapeData;
        gameObject.name = $"Shape_{shapeData.name}";
        foreach (Transform child in transform) Destroy(child.gameObject);
        _marbles.Clear();
        var localPositions = new List<Vector3>();
        foreach (var gridPos in shapeData.MarblePositions){
            float worldX = gridPos.x * hSpacing + (gridPos.y % 2 != 0 ? hSpacing / 2f : 0);
            float worldY = gridPos.y * vSpacing;
            localPositions.Add(new Vector3(worldX, worldY, 0));
        }
        Vector3 centerOffset = Vector3.zero;
        if (localPositions.Count > 0){
            foreach (var pos in localPositions) centerOffset += pos;
            centerOffset /= localPositions.Count;
        }
        foreach (var pos in localPositions){
            GameObject marbleObj = Instantiate(marblePrefab, this.transform);
            marbleObj.transform.localPosition = pos - centerOffset;
            Marble newMarble = marbleObj.GetComponent<Marble>();
            Color randomColor = palette.Colors[Random.Range(0, palette.Colors.Count)];
            newMarble.SetColor(randomColor);
            _marbles.Add(newMarble);
        }
    }
    private void RunCoroutine(IEnumerator routine){
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(routine);
    }
    private IEnumerator PickupRoutine(){
        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + pickupOffset;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = _originalScale * pickupScale;
        float timer = 0f;
        while (timer < scaleDuration){
            float t = timer / scaleDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        transform.localScale = targetScale;
    }
    private IEnumerator ReturnRoutine(){
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float timer = 0f;
        while (timer < scaleDuration){
            float t = timer / scaleDuration;
            transform.position = Vector3.Lerp(startPos, _originalPosition, t);
            transform.localScale = Vector3.Lerp(startScale, _originalScale, t);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = _originalPosition;
        transform.localScale = _originalScale;
        transform.SetParent(OriginalParent);
    }
    #endregion
}