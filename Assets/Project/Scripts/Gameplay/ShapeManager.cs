using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct IntelligentSpawn
{
    public ShapeData_SO ShapeData;
    public Dictionary<Vector2Int, Color> OverrideColors; 
}

public class ShapeManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GridManager gridManager;
    
    [Header("Şekil Veritabanı")]
    [SerializeField] private List<ShapeData_SO> allShapesInOrder;

    [Header("Prefabs")]
    [SerializeField] private Shape shapePrefab;
    [SerializeField] private GameObject marblePrefab;

    [Header("Hazne (Queue) Ayarları")]
    [SerializeField] private Transform[] queueSlots;
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;

    [Header("Dinamik Zorluk Ayarları")]
    [Tooltip("Izgaradaki boş nokta yüzdesi bu değerin altına düştüğünde YARDIM sistemi devreye girebilir.")]
    [Range(0f, 100f)]
    [SerializeField] private float assistanceThreshold = 30f;
    
    [Tooltip("Yardım sistemi koşulları sağlandığında, yardım etmenin gerçekleşme olasılığı (Yüzde).")]
    [Range(0f, 100f)]
    [SerializeField] private float assistanceChance = 50f;

    [Tooltip("Izgaradaki boş nokta yüzdesi bu değerin üzerindeyken KÖSTEK sistemi devreye girebilir.")]
    [Range(0f, 100f)]
    [SerializeField] private float hindranceThreshold = 70f;
    
    [Tooltip("Köstek sistemi koşulları sağlandığında, köstek olmanın gerçekleşme olasılığı (Yüzde).")]
    [Range(0f, 100f)]
    [SerializeField] private float hindranceChance = 50f;
    
    private LevelData_SO _currentLevelData;
    private int _shapesLeftInQueue;
    private int _currentDisplayLevel; 
    
    private enum SpawnMode { Assistance, Hindrance, Random }

    private void OnEnable()
    {
        EventManager.OnTurnCompleted += HandleTurnCompleted;
    }

    private void OnDisable()
    {
        EventManager.OnTurnCompleted -= HandleTurnCompleted;
    }
    public void PrepareInitialShapes(LevelData_SO levelData, int displayLevel)
    {
        if (!ValidateSettings()) return;
        _currentLevelData = levelData;
        _currentDisplayLevel = displayLevel;
        
        foreach (var slot in queueSlots) { foreach (Transform child in slot) Destroy(child.gameObject); }
        
        StartCoroutine(SpawnBatchWithDelay(0f));
    }
    public void RefreshShapeQueue()
    {
        if (!gameObject.activeInHierarchy) return;
        foreach (var slot in queueSlots)
        {
            foreach (Transform child in slot) Destroy(child.gameObject);
        }
        
        StartCoroutine(SpawnBatchWithDelay(0f));
    }
    
    private void HandleTurnCompleted()
    {
        _shapesLeftInQueue--;
        CheckForLoseCondition();

        if (_shapesLeftInQueue <= 0)
        {
            StartCoroutine(SpawnBatchWithDelay(0.5f));
        }
    }
    
    private IEnumerator SpawnBatchWithDelay(float delay, bool forceAssistance = false)
    {
        yield return new WaitForSeconds(delay);
        SpawnNewShapeBatch(forceAssistance);
        
        yield return null; 
        CheckForLoseCondition();
    }
    
    private void SpawnNewShapeBatch(bool forceAssistance)
    {
        if (queueSlots.Length == 0) return;
        _shapesLeftInQueue = queueSlots.Length;
        
        var availableShapes = GetAvailableShapesForCurrentLevel();
        SpawnMode currentMode = DecideSpawnMode(forceAssistance);

        switch (currentMode)
        {
            case SpawnMode.Assistance:
                Debug.Log("<color=green>SPAWN MODU: Yardımcı</color>");
                SpawnAssistanceBatch(availableShapes);
                break;
            case SpawnMode.Hindrance:
                Debug.Log("<color=red>SPAWN MODU: Köstek</color>");
                SpawnHindranceBatch(availableShapes);
                break;
            case SpawnMode.Random:
                Debug.Log("<color=yellow>SPAWN MODU: Rastgele</color>");
                SpawnRandomBatch(availableShapes);
                break;
        }
    }

    private SpawnMode DecideSpawnMode(bool forceAssistance)
    {
        if (forceAssistance) return SpawnMode.Assistance;

        int totalNodes = gridManager.GetGrid().Count(n => !n.Value.IsLocked && n.Value.gameObject.activeSelf);
        int availableNodeCount = gridManager.GetGrid().Values.Count(n => n.IsAvailable);
        float availablePercentage = totalNodes > 0 ? ((float)availableNodeCount / totalNodes) * 100f : 0f;

        if (availablePercentage <= assistanceThreshold)
        {
            if (Random.Range(0f, 100f) <= assistanceChance)
            {
                return SpawnMode.Assistance;
            }
        }
        if (_currentDisplayLevel > 10 && availablePercentage >= hindranceThreshold)
        {
            if (Random.Range(0f, 100f) <= hindranceChance)
            {
                return SpawnMode.Hindrance;
            }
        }
        
        return SpawnMode.Random;
    }

    private void SpawnAssistanceBatch(List<ShapeData_SO> availableShapes)
    {
        var shapePool = new List<ShapeData_SO>(availableShapes);
        IntelligentSpawn helpfulSpawn = FindBestPossibleMove(shapePool);
        
        if (helpfulSpawn.ShapeData != null)
        {
            shapePool.Remove(helpfulSpawn.ShapeData);
        }
        
        int helpfulSlotIndex = Random.Range(0, queueSlots.Length);

        for (int i = 0; i < queueSlots.Length; i++)
        {
            if (i == helpfulSlotIndex && helpfulSpawn.ShapeData != null)
            {
                SpawnSingleShape(queueSlots[i], helpfulSpawn);
            }
            else
            {
                if (shapePool.Any())
                {
                    int randomIndex = Random.Range(0, shapePool.Count);
                    var randomShape = shapePool[randomIndex];
                    SpawnSingleShape(queueSlots[i], new IntelligentSpawn { ShapeData = randomShape, OverrideColors = null });
                    
                    shapePool.RemoveAt(randomIndex);
                }
                else
                {
                    SpawnSingleShape(queueSlots[i], new IntelligentSpawn { ShapeData = allShapesInOrder[0], OverrideColors = null });
                }
            }
        }
    }

    private void SpawnHindranceBatch(List<ShapeData_SO> availableShapes)
    {
        var shapePool = new List<ShapeData_SO>(availableShapes);

        for (int i = 0; i < queueSlots.Length; i++)
        {
            if (!shapePool.Any())
            {
                SpawnSingleShape(queueSlots[i], new IntelligentSpawn { ShapeData = allShapesInOrder[0], OverrideColors = null });
                continue;
            }
            
            IntelligentSpawn worstSpawn = FindWorstPossibleMove(shapePool);
            SpawnSingleShape(queueSlots[i], worstSpawn);
            
            if (worstSpawn.ShapeData != null)
            {
                shapePool.Remove(worstSpawn.ShapeData);
            }
        }
    }
    
    private void SpawnRandomBatch(List<ShapeData_SO> availableShapes)
    {
        foreach (var slot in queueSlots)
        {
            var randomShape = availableShapes[Random.Range(0, availableShapes.Count)];
            SpawnSingleShape(slot, new IntelligentSpawn { ShapeData = randomShape, OverrideColors = null });
        }
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
        
        newShape.Initialize(spawnInfo.ShapeData, _currentLevelData.AvailableColors, marblePrefab, horizontalSpacing, verticalSpacing, spawnInfo.OverrideColors);
    }
    

    private IntelligentSpawn FindBestPossibleMove(List<ShapeData_SO> shapes)
    {
        var allAvailableNodes = gridManager.GetGrid().Values.Where(n => n.IsAvailable).ToList();
        
        if (allAvailableNodes.Count < 2)
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
                    
                    foreach (var colorKvp in neighborColors)
                    {
                        if (colorKvp.Value + shape.MarblePositions.Count >= 5)
                        {
                            var overrideColors = new Dictionary<Vector2Int, Color>();
                            overrideColors[shape.MarblePositions[0]] = colorKvp.Key;
                            
                            return new IntelligentSpawn { ShapeData = shape, OverrideColors = overrideColors };
                        }
                    }
                }
            }
        }
        return new IntelligentSpawn { ShapeData = null, OverrideColors = null }; 
    }
    
   private IntelligentSpawn FindWorstPossibleMove(List<ShapeData_SO> shapes)
    {
        var allAvailableNodes = gridManager.GetGrid().Values.Where(n => n.IsAvailable).ToList();
        
        var gridColorCounts = gridManager.GetGrid().Values
            .Where(n => n.IsOccupied)
            .GroupBy(n => n.PlacedMarble.MarbleColor)
            .ToDictionary(g => g.Key, g => g.Count());

        var minorityColors = _currentLevelData.AvailableColors.Colors
            .OrderBy(c => gridColorCounts.ContainsKey(c) ? gridColorCounts[c] : 0)
            .ToList();

        var possibleMoves = new List<(ShapeData_SO shape, Vector2Int anchor, Dictionary<Vector2Int, Color> assignedColors, int score)>();

        var placeableShapes = shapes.Where(s => gridManager.CanShapeBePlacedAnywhere(s)).ToList();
        if (!placeableShapes.Any()) placeableShapes.Add(allShapesInOrder[0]); 

        foreach (var shape in placeableShapes.OrderByDescending(s => s.MarblePositions.Count)) 
        {
            foreach (var startNode in allAvailableNodes)
            {
                var placementNodes = gridManager.GetPlacementNodes(shape, startNode.GridPosition);

                if (placementNodes.Count == shape.MarblePositions.Count && placementNodes.All(n => n.IsAvailable))
                {
                    int moveScore = 0;
                    var assignedColors = new Dictionary<Vector2Int, Color>();
                    var availableMinorityColors = new Queue<Color>(minorityColors);

                    foreach (var node in placementNodes)
                    {
                        var neighbors = gridManager.GetNeighbors(node);
                        var neighborGroups = neighbors
                            .Where(n => n.IsOccupied && !placementNodes.Contains(n))
                            .GroupBy(n => n.PlacedMarble.MarbleColor);
                        
                        var mostDangerousColorGroup = neighborGroups.OrderByDescending(g => g.Count()).FirstOrDefault();
                        
                        Color colorToAssign;
                        if (mostDangerousColorGroup != null && mostDangerousColorGroup.Count() >= 2)
                        {
                            colorToAssign = availableMinorityColors.Dequeue();
                            availableMinorityColors.Enqueue(colorToAssign); 
                            moveScore += 5; 
                        }
                        else
                        {
                            colorToAssign = minorityColors[Random.Range(0, minorityColors.Count)];
                            moveScore += 1;
                        }
                        
                        var localPos = node.GridPosition - startNode.GridPosition;
                        assignedColors[localPos] = colorToAssign;
                    }
                    possibleMoves.Add((shape, startNode.GridPosition, assignedColors, moveScore));
                }
            }
        }

        if (possibleMoves.Any())
        {
            var worstMove = possibleMoves.OrderByDescending(m => m.score).First();
            return new IntelligentSpawn { ShapeData = worstMove.shape, OverrideColors = worstMove.assignedColors };
        }

        return new IntelligentSpawn { ShapeData = allShapesInOrder[0], OverrideColors = null };
    }

    
    private List<ShapeData_SO> GetAvailableShapesForCurrentLevel()
    {
        int shapeCountToUse;

        if (_currentDisplayLevel > 40) 
        {
            shapeCountToUse = allShapesInOrder.Count;
        }
        else if (_currentDisplayLevel <= 10) shapeCountToUse = 4;
        else if (_currentDisplayLevel <= 20) shapeCountToUse = 7;
        else if (_currentDisplayLevel <= 30) shapeCountToUse = 10;
        else 
        {
            shapeCountToUse = allShapesInOrder.Count;
        }

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
            Debug.Log("<color=orange>LOSE KONTROLÜ: Yerleştirilecek uygun yer bulunamadı. Oyun Bitti.</color>");
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