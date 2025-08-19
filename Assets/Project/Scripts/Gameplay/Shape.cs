using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [Header("Görsel Efektler")]
    [SerializeField] private float pickupScale = 1.2f;
    [SerializeField] private float scaleDuration = 0.1f;
    [SerializeField] private Vector3 pickupOffset = new Vector3(0, 0.5f, 0);
    public ShapeData_SO ShapeData { get; private set; }
    public bool IsPlaced { get; private set; }
    public Transform OriginalParent { get; private set; }

    private List<Marble> _marbles = new List<Marble>();
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private GridManager _gridManager;
    private GhostController _activeGhost; 
    private Coroutine _activeCoroutine;
    private Dictionary<Marble, GridNode> _lastValidPlacement;
    private List<GridNode> _lastMarkedNodes = new List<GridNode>();
    private GridNode _lastClosestNode = null;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _gridManager = FindObjectOfType<GridManager>();
    }
    
    public void OnDrag(Vector3 newPosition)
    {
        transform.position = newPosition + pickupOffset;

        GridNode currentClosestNode = _gridManager.GetClosestNode(_marbles[0].transform.position);

        if (currentClosestNode == _lastClosestNode)
        {
            return;
        }

        _lastClosestNode = currentClosestNode;
        ResetMarkedNodes();

        if (currentClosestNode != null)
        {
            var targetPlacement = _gridManager.GetTargetPlacement(this);
            bool isValid = _gridManager.CheckPlacementValidity(targetPlacement);
            
            if (isValid)
            {
                _activeGhost.gameObject.SetActive(true);
                _activeGhost.UpdatePositions(targetPlacement);
                _lastValidPlacement = targetPlacement;
            }
            else
            {
                _activeGhost.gameObject.SetActive(false);
                MarkNodes(targetPlacement.Values, Color.red);
                _lastValidPlacement = null;
            }
        }
        else
        {
            _activeGhost.gameObject.SetActive(false);
            _lastValidPlacement = null;
        }
    }

    public void OnSelected()
    {
        _originalPosition = transform.position;
        OriginalParent = transform.parent;
        
        _activeGhost = GhostPoolManager.Instance.GetGhost();
        _activeGhost.Initialize(this);
        
        _activeGhost.gameObject.SetActive(false);
        ResetMarkedNodes();
        _lastClosestNode = null; 

        RunCoroutine(PickupRoutine());
    }

    public void OnDropped()
    {
        ResetMarkedNodes();
        
        if (_lastValidPlacement != null)
        {
            _gridManager.PlaceShape(this, _lastValidPlacement);
            IsPlaced = true;
            this.enabled = false;
            GhostPoolManager.Instance.ReturnGhost(_activeGhost);
        }
        else
        {
            RunCoroutine(ReturnRoutine());
        }
        _lastClosestNode = null;
    }
    
    private IEnumerator ReturnRoutine()
    {
        if (_activeGhost != null)
        {
            GhostPoolManager.Instance.ReturnGhost(_activeGhost);
            _activeGhost = null;
        }

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
    
    #region Değişmeyen Kodlar
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
    public List<Marble> GetMarbles() => _marbles;
    public void Initialize(ShapeData_SO shapeData, ColorPalette_SO colorPalette, GameObject marblePrefab, float hSpacing, float vSpacing, Dictionary<Vector2Int, Color> overrideColors)
    {
        this.ShapeData = shapeData;
        _gridManager = FindObjectOfType<GridManager>();

        _originalScale = transform.localScale;

        var availableColors = colorPalette.Colors;
        GameObject pivot = new GameObject("Pivot");
        pivot.transform.SetParent(this.transform, false); 

        Vector3 totalPosition = Vector3.zero;
        List<Vector3> marbleWorldPositions = new List<Vector3>();

        foreach (var pos in shapeData.MarblePositions)
        {
            float worldX = pos.x * hSpacing + (pos.y % 2 != 0 ? hSpacing / 2f : 0);
            float worldY = pos.y * vSpacing;
            Vector3 marblePos = new Vector3(worldX, worldY, 0);
            
            marbleWorldPositions.Add(marblePos);
            totalPosition += marblePos;
        }

        Vector3 centerOffset = totalPosition / shapeData.MarblePositions.Count;
        pivot.transform.localPosition = -centerOffset;
        
        for (int i = 0; i < shapeData.MarblePositions.Count; i++)
        {
            var pos = shapeData.MarblePositions[i];
            GameObject marbleObj = Instantiate(marblePrefab, pivot.transform);
            marbleObj.transform.localPosition = marbleWorldPositions[i];
            
            Marble newMarble = marbleObj.GetComponent<Marble>();
            
            if (overrideColors != null && overrideColors.TryGetValue(pos, out Color specificColor))
            {
                newMarble.SetColor(specificColor);
            }
            else
            {
                newMarble.SetColor(availableColors[Random.Range(0, availableColors.Count)]);
            }
            
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
    #endregion
}