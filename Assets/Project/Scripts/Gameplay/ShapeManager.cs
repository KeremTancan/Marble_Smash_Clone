using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct IntelligentSpawn
{
    public ShapeData_SO ShapeData;
    public Color? OverrideColor;
}

public class ShapeManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GridManager gridManager;
    
    [Header("Şekil Veritabanı")]
    [Tooltip("Tüm şekillerin ZORLUK SIRASINA GÖRE eklendiği ana liste. 0 = en kolay.")]
    [SerializeField] private List<ShapeData_SO> allShapesInOrder;

    [Header("Prefabs")]
    [SerializeField] private Shape shapePrefab;
    [SerializeField] private GameObject marblePrefab;

    [Header("Hazne (Queue) Ayarları")]
    [SerializeField] private Transform[] queueSlots;
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;

    [Header("Akıllı Spawn Ayarları")]
    [Tooltip("Izgaradaki boş nokta yüzdesi bu değerin altına düştüğünde yardım sistemi devreye girer.")]
    [Range(0, 100)]
    [SerializeField] private float helpPercentageThreshold = 40f;
    
    private LevelData_SO _currentLevelData;
    private int _shapesLeftInQueue;

    private void OnEnable()
    {
        EventManager.OnTurnCompleted += HandleTurnCompleted;
    }

    private void OnDisable()
    {
        EventManager.OnTurnCompleted -= HandleTurnCompleted;
    }
    
    public void PrepareInitialShapes(LevelData_SO levelData)
    {
        if (!ValidateSettings()) return;
        _currentLevelData = levelData;
        
        foreach (var slot in queueSlots) { foreach (Transform child in slot) Destroy(child.gameObject); }
        
        SpawnNewShapeBatch(false);
        CheckForLoseCondition();
    }
    
    public void RefreshShapeQueue()
    {
        if (!gameObject.activeInHierarchy) return;
        foreach (var slot in queueSlots)
        {
            foreach (Transform child in slot) Destroy(child.gameObject);
        }
        StartCoroutine(SpawnBatchWithDelay(0f, true));
    }

    private void HandleTurnCompleted()
    {
        _shapesLeftInQueue--;

        if (_shapesLeftInQueue > 0)
        {
            CheckForLoseCondition();
        }
        else
        {
            StartCoroutine(SpawnBatchWithDelay(0.5f, false));
        }
    }
    
    private IntelligentSpawn GetNextShape(bool forceHelp)
    {
        var availableShapes = GetAvailableShapesForCurrentLevel();
        
        int totalNodes = gridManager.GetGrid().Count(n => !n.Value.IsLocked);
        int availableNodeCount = gridManager.GetGrid().Values.Count(n => n.IsAvailable);
        float availablePercentage = totalNodes > 0 ? ((float)availableNodeCount / totalNodes) * 100f : 0f;

        bool needsHelp = (availablePercentage <= helpPercentageThreshold) || forceHelp;
        
        string logMessage = $"<color=cyan>AKILLI SPAWN KONTROLÜ:</color> Izgara Doluluğu: %{100 - availablePercentage:F1} (Eşik: %{100-helpPercentageThreshold}). Yardım Zorlandı: {forceHelp}.";
        Debug.Log(logMessage);

        if (needsHelp)
        {
            var bestSpawn = FindBestPossibleMove(availableShapes);
            if (bestSpawn.ShapeData != null)
            {
                Debug.Log("<color=green>SONUÇ: Yardımcı hamle bulundu ve veriliyor.</color>");
                return bestSpawn;
            }
        }
        
        Debug.Log("<color=yellow>SONUÇ: Yardım gerekmiyor veya bulunamadı, rastgele şekil veriliyor.</color>");
        var randomShape = availableShapes[Random.Range(0, availableShapes.Count)];
        return new IntelligentSpawn { ShapeData = randomShape, OverrideColor = null };
    }
    
    private IntelligentSpawn FindBestPossibleMove(List<ShapeData_SO> shapes)
    {
        var allAvailableNodes = gridManager.GetGrid().Values.Where(n => n.IsAvailable).ToList();
        
        if (allAvailableNodes.Count < 4)
        {
            shapes = shapes.Where(s => s.MarblePositions.Count <= allAvailableNodes.Count).ToList();
        }

        foreach (var shape in shapes.OrderBy(s => Random.value))
        {
            foreach (var startNode in allAvailableNodes)
            {
                var placementNodes = gridManager.GetPlacementNodes(shape, startNode.GridPosition);

                if (placementNodes.Count == shape.MarblePositions.Count && placementNodes.All(n => n.IsAvailable))
                {
                    var neighborColors = gridManager.GetNeighboringColors(placementNodes);
                    var bestColor = neighborColors.OrderByDescending(kvp => kvp.Value).FirstOrDefault();

                    if (bestColor.Key != default(Color) && (bestColor.Value + shape.MarblePositions.Count) >= 5)
                    {
                        return new IntelligentSpawn { ShapeData = shape, OverrideColor = bestColor.Key };
                    }
                }
            }
        }
        return new IntelligentSpawn { ShapeData = null, OverrideColor = null };
    }
    
    private void SpawnSingleShape(Transform slot, IntelligentSpawn spawnInfo)
    {
        if (slot == null || _currentLevelData == null) return;
        
        if (spawnInfo.ShapeData == null)
        {
             Debug.LogWarning("Spawn edilecek uygun bir şekil bulunamadı, en basit şekil seçiliyor.");
             spawnInfo.ShapeData = allShapesInOrder[0];
        }

        Shape newShape = Instantiate(shapePrefab, slot);
        newShape.transform.localPosition = Vector3.zero;
        
        newShape.Initialize(spawnInfo.ShapeData, _currentLevelData.AvailableColors, marblePrefab, horizontalSpacing, verticalSpacing, spawnInfo.OverrideColor);
    }
    
    private void SpawnNewShapeBatch(bool forceHelp)
    {
        if (queueSlots.Length == 0) return;
        
        _shapesLeftInQueue = queueSlots.Length;
        
        foreach (var slot in queueSlots) { 
            IntelligentSpawn spawnInfo = GetNextShape(forceHelp);
            SpawnSingleShape(slot, spawnInfo);
        }
    }

    private IEnumerator SpawnBatchWithDelay(float delay, bool forceHelp)
    {
        yield return new WaitForSeconds(delay);
        SpawnNewShapeBatch(forceHelp);
        CheckForLoseCondition();
    }
    
    private List<ShapeData_SO> GetAvailableShapesForCurrentLevel()
    {
        int levelId = _currentLevelData.LevelID;
        int shapeCountToUse;

        if (levelId <= 10) shapeCountToUse = 5;
        else if (levelId <= 20) shapeCountToUse = 7;
        else if (levelId <= 30) shapeCountToUse = 9;
        else if (levelId <= 40) shapeCountToUse = 11;
        else shapeCountToUse = allShapesInOrder.Count;

        shapeCountToUse = Mathf.Min(shapeCountToUse, allShapesInOrder.Count);
        return allShapesInOrder.GetRange(0, shapeCountToUse);
    }
    
    private void CheckForLoseCondition()
    {
        var remainingShapes = new List<Shape>();
        foreach (var slot in queueSlots)
        {
            var shapeInSlot = slot.GetComponentInChildren<Shape>();
            if (shapeInSlot != null) remainingShapes.Add(shapeInSlot);
        }

        if (remainingShapes.Count == 0) return;

        bool canAnyShapeBePlaced = false;
        foreach (var shape in remainingShapes)
        {
            if (gridManager.CanShapeBePlacedAnywhere(shape.ShapeData))
            {
                canAnyShapeBePlaced = true;
                break;
            }
        }

        if (!canAnyShapeBePlaced)
        {
            EventManager.RaiseOnLevelFailed();
        }
    }

    private bool ValidateSettings()
    {
        if(gridManager == null) { Debug.LogError("ShapeManager'a GridManager referansı atanmamış!", this); return false; }
        if (allShapesInOrder == null || allShapesInOrder.Count == 0) { Debug.LogError("ShapeManager'da 'All Shapes In Order' listesi boş!", this); return false; }
        if (shapePrefab == null || marblePrefab == null) { Debug.LogError("ShapeManager'a 'Shape' veya 'Marble' prefab'ı atanmamış!", this); return false; }
        if (queueSlots == null || queueSlots.Length == 0) { Debug.LogError("ShapeManager'a 'Queue Slots' atanmamış!", this); return false; }
        return true;
    }
}