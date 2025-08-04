using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GridManager gridManager;
    
    [Header("Şekil Veritabanı")]
    [SerializeField] private List<ShapeData_SO> allAvailableShapes;

    [Header("Prefabs")]
    [SerializeField] private Shape shapePrefab;
    [SerializeField] private GameObject marblePrefab;

    [Header("Hazne (Queue) Ayarları")]
    [SerializeField] private Transform[] queueSlots;

    [Header("Izgara Ayarları (Görsel için)")]
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;

    private ColorPalette_SO _currentPalette;
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
        _currentPalette = levelData.AvailableColors;
        foreach (var slot in queueSlots) { foreach (Transform child in slot) Destroy(child.gameObject); }
        
        // Başlangıçta yeni şekilleri oluştur ve HEMEN KONTROL ET
        SpawnNewShapeBatch();
        CheckForLoseCondition();
    }

    private void HandleTurnCompleted()
    {
        _shapesLeftInQueue--;

        if (_shapesLeftInQueue > 0)
        {
            // Eğer slotlarda hala şekil varsa, durumu kontrol et
            CheckForLoseCondition();
        }
        else
        {
            // Eğer slotlar boşaldıysa, yenilerini getirmesi için Coroutine'i başlat
            StartCoroutine(SpawnBatchWithDelay(0.5f));
        }
    }
    
    private void CheckForLoseCondition()
    {
        var remainingShapes = new List<Shape>();
        foreach (var slot in queueSlots)
        {
            Shape shapeInSlot = slot.GetComponentInChildren<Shape>();
            if (shapeInSlot != null)
            {
                remainingShapes.Add(shapeInSlot);
            }
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

    private void SpawnNewShapeBatch()
    {
        if (queueSlots.Length == 0) return;
        foreach (var slot in queueSlots) { SpawnSingleShape(slot); }
        _shapesLeftInQueue = queueSlots.Length;
    }

    private void SpawnSingleShape(Transform slot)
    {
        if (slot == null || _currentPalette == null) return;
        ShapeData_SO randomShapeData = allAvailableShapes[Random.Range(0, allAvailableShapes.Count)];
        Shape newShape = Instantiate(shapePrefab, slot);
        newShape.transform.localPosition = Vector3.zero;
        newShape.Initialize(randomShapeData, _currentPalette, marblePrefab, horizontalSpacing, verticalSpacing);
    }
    
    private IEnumerator SpawnBatchWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Yeni şekilleri oluştur
        SpawnNewShapeBatch();
        // VE YENİ ŞEKİLLER GELDİKTEN SONRA TEKRAR KONTROL ET
        CheckForLoseCondition();
    }

    private bool ValidateSettings()
    {
        if(gridManager == null) { Debug.LogError("ShapeManager'a GridManager referansı atanmamış!", this); return false; }
        if (allAvailableShapes == null || allAvailableShapes.Count == 0) { Debug.LogError("ShapeManager'da 'All Available Shapes' listesi boş!", this); return false; }
        if (shapePrefab == null || marblePrefab == null) { Debug.LogError("ShapeManager'a 'Shape' veya 'Marble' prefab'ı atanmamış!", this); return false; }
        if (queueSlots == null || queueSlots.Length == 0) { Debug.LogError("ShapeManager'a 'Queue Slots' atanmamış!", this); return false; }
        return true;
    }
}