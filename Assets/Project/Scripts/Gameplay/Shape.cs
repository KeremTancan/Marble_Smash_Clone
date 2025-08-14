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
    private GridNode _lastClosestNode = null;

    private void OnDestroy()
    {
        if (_ghostInstance != null) Destroy(_ghostInstance.gameObject);
    }

    private void Awake()
    {
        _originalScale = transform.localScale;
        _gridManager = FindFirstObjectByType<GridManager>();
    }
    
    public void Initialize(ShapeData_SO shapeData, ColorPalette_SO palette, GameObject marblePrefab, float hSpacing, float vSpacing, Color? overrideColor = null)
    {
        this.ShapeData = shapeData;
        gameObject.name = $"Shape_{shapeData.name}";
        foreach (Transform child in transform) Destroy(child.gameObject);
        _marbles.Clear();
        var localPositions = new List<Vector3>();
        foreach (var gridPos in shapeData.MarblePositions) {
            float worldX = gridPos.x * hSpacing + (gridPos.y % 2 != 0 ? hSpacing / 2f : 0);
            float worldY = gridPos.y * vSpacing;
            localPositions.Add(new Vector3(worldX, worldY, 0));
        }
        Vector3 centerOffset = Vector3.zero;
        if (localPositions.Count > 0) {
            foreach (var pos in localPositions) centerOffset += pos;
            centerOffset /= localPositions.Count;
        }
        
        bool useOverrideColor = overrideColor.HasValue;

        foreach (var pos in localPositions) {
            GameObject marbleObj = Instantiate(marblePrefab, this.transform);
            marbleObj.transform.localPosition = pos - centerOffset;
            marbleObj.transform.localScale = marblePrefab.transform.localScale;
            
            Marble newMarble = marbleObj.GetComponent<Marble>();
            Color colorToSet = useOverrideColor ? overrideColor.Value : palette.Colors[Random.Range(0, palette.Colors.Count)];
            newMarble.SetColor(colorToSet);
            _marbles.Add(newMarble);
        }
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
        _lastClosestNode = null;
        
        RunCoroutine(PickupRoutine());
    }

    public void OnDrag(Vector3 newPosition)
    {
        transform.position = newPosition;

        GridNode currentClosestNode = _gridManager.GetClosestNode(transform.position);
        if (currentClosestNode == _lastClosestNode) return;
        
        _lastClosestNode = currentClosestNode;
        ResetMarkedNodes();
        _lastValidPlacement = null;

        if (currentClosestNode != null)
        {
            var targetPlacement = _gridManager.GetTargetPlacement(this);
            
            if (targetPlacement.Count == _marbles.Count && _gridManager.CheckPlacementValidity(targetPlacement))
            {
                _ghostInstance.gameObject.SetActive(true);
                _ghostInstance.UpdatePositions(targetPlacement);
                _lastValidPlacement = targetPlacement;
            }
            else
            {
                _ghostInstance.gameObject.SetActive(false);
                if (targetPlacement.Count > 0)
                {
                    MarkNodes(targetPlacement.Values, Color.red);
                }
            }
        }
        else
        {
            _ghostInstance.gameObject.SetActive(false);
        }
    }

    public void OnDropped()
    {
        ResetMarkedNodes();
        if (_ghostInstance != null) _ghostInstance.gameObject.SetActive(false);

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
        _lastClosestNode = null;
    }
    
    public List<Marble> GetMarbles() => _marbles;

    private void RunCoroutine(IEnumerator routine)
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(routine);
    }

    private IEnumerator PickupRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + pickupOffset;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = _originalScale * pickupScale;
        float timer = 0f;
        while (timer < scaleDuration)
        {
            float t = timer / scaleDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        transform.localScale = targetScale;
    }

    private IEnumerator ReturnRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float timer = 0f;
        while (timer < scaleDuration)
        {
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
    
    private void MarkNodes(IEnumerable<GridNode> nodes, Color color)
    {
        foreach (var node in nodes)
        {
            if(node != null)
            {
                node.SetHighlightColor(color);
                _lastMarkedNodes.Add(node);
            }
        }
    }

    private void ResetMarkedNodes()
    {
        foreach (var node in _lastMarkedNodes)
        {
            if(node != null) node.ResetColor();
        }
        _lastMarkedNodes.Clear();
    }
}