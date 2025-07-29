using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
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
        EventManager.OnShapePlaced += HandleShapePlaced;
    }

    private void OnDisable()
    {
        EventManager.OnShapePlaced -= HandleShapePlaced;
    }

    public void PrepareInitialShapes(LevelData_SO levelData)
    {
        if (!ValidateSettings()) return;

        _currentPalette = levelData.AvailableColors;
        
        foreach (var slot in queueSlots)
        {
            foreach (Transform child in slot) Destroy(child.gameObject);
        }

        SpawnNewShapeBatch();
    }

    
    private void HandleShapePlaced() // Bir şekil kullanıldığında bu metot çağrılır
    {
        _shapesLeftInQueue--; 

        if (_shapesLeftInQueue <= 0)
        {
            SpawnNewShapeBatch();
        }
    }
    private void SpawnNewShapeBatch()
    {
        if (queueSlots.Length == 0) return;

        foreach (var slot in queueSlots)
        {
            SpawnSingleShape(slot);
        }
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
    
    

    private bool ValidateSettings()
    {
        if (allAvailableShapes == null || allAvailableShapes.Count == 0) { Debug.LogError("ShapeManager'da 'All Available Shapes' listesi boş!", this); return false; }
        if (shapePrefab == null || marblePrefab == null) { Debug.LogError("ShapeManager'a 'Shape' veya 'Marble' prefab'ı atanmamış!", this); return false; }
        if (queueSlots == null || queueSlots.Length == 0) { Debug.LogError("ShapeManager'a 'Queue Slots' atanmamış!", this); return false; }
        return true;
    }
}