using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Oyun ızgarasını yönetir. Seviye verisine göre ızgarayı oluşturur,
/// belirtilen güvenli alana sığacak şekilde ölçekler, ortalar ve bağlantıları çizer.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Seviye Verisi")]
    [SerializeField] private LevelData_SO currentLevelData;

    [Header("Prefabs")]
    [SerializeField] private GridNode gridNodePrefab;
    [SerializeField] private LineRenderer connectionPrefab;

    // ====================================================================================
    // --- BURAYI DÜZENLE: Izgaranın sığdırılacağı güvenli alanı buradan ayarlayabilirsin ---
    [Header("Güvenli Alan Ayarları")]
    [Tooltip("Izgaranın sığdırılacağı alan. Geniş veya uzun ızgaralar bu alana sığacak şekilde küçültülür.")]
    [SerializeField] private Rect safeArea = new Rect(-4f, -4f, 8f, 8f);
    // ====================================================================================

    [Header("Izgara Ayarları")]
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;

    private readonly Dictionary<Vector2Int, GridNode> _grid = new Dictionary<Vector2Int, GridNode>();
    private Transform _connectionsParent;

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

        // Aşama 1: Node'ları varsayılan pozisyonlarında oluştur.
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
        
        // Aşama 2: Izgarayı ölçekle ve ortala.
        FitGridToSafeArea();

        // Aşama 3: Node'lar nihai pozisyonlarına ulaştıktan sonra bağlantıları çiz.
        foreach (GridNode node in _grid.Values)
        {
            ConnectToNeighbors(node);
        }
    }

    /// <summary>
    /// Oluşturulan ızgarayı, 'safeArea' içine sığacak şekilde ölçekler ve ortalar.
    /// </summary>
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
            gridBounds = GetGridBounds();
        }

        // DÜZELTİLEN SATIR: safeArea.center'ı Vector3'e dönüştürüyoruz.
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
}
